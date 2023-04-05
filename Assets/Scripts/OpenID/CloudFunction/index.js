const Firestore = require('@google-cloud/firestore');
const crypto = require('crypto');

//entry point
exports.openid = async (req, res) => {
  if(req.path == "/token"){
    await getOneToken(req,res);
  }else if(req.path == "/tokens"){
    await getMultipleTokens(req, res);
  }else if(req.path == "/userinfo"){
    await getUserInfo(req, res);
  }else{
    res.status(404).send("Wrong path");
  }  
};

//UserInfo OpenID endpoint called by EOS to verify token
async function getUserInfo(req, res){
  let auth = req.get('Authorization') || "";
  let authParts = auth.split(' ');
  if(authParts[0] == "Bearer" && authParts[1]){
    let headerEnc, payloadEnc, signature;
    [headerEnc, payloadEnc, signature] = authParts[1].split('.');
    let header = JSON.parse(Buffer.from(headerEnc, 'base64url').toString());
    let payload = JSON.parse(Buffer.from(payloadEnc, 'base64url').toString());
    if(header.alg == "HS256" && payload.iss == "gcp" && payload.aud == "eos"){
      let sigVerify = crypto.createHmac('sha256', process.env.SECRET).update(headerEnc+'.'+payloadEnc).digest('base64url');
      if(sigVerify == signature){
        const db = new Firestore();
        const accountRef = db.collection('eos-accounts').doc(payload.sub);
        const doc = await accountRef.get();
        if(doc.exists){
          let userInfo = {
            sub:payload.sub
          };
          res.status(200).type('application/json').send(JSON.stringify(userInfo));
          return;
        }else{
          res.status(404).send();
        }
      }
    }
  }

  res.status(401).send();
}

//Gets one token using Basic auth
async function getOneToken(req, res){
  let auth = req.get('Authorization') || "";
  let authParts = auth.split(' ');
  let username, password;
  if(authParts[0] == "Basic" && authParts[1]){
    let buff = Buffer.from(authParts[1], 'base64url');
    let text = buff.toString();
    [username,password] = text.split(':');
  }else{
    res.status(401).send("Basic auth required");
    return;
  }
  if(!username || !password){
    res.status(401).send("Username or password missing");
    return;
  }

  let requestData = {requests:[{"username":username,"password":password}]};
  let resultData = await getTokens(requestData);
  res.status(200).type('application/json').send(JSON.stringify(resultData));
}

//Gets multiple tokens from a JSON collection of usernames/passwords
async function getMultipleTokens(req, res){
  if(!Array.isArray(req.body.requests)){
    res.status(400).send();
  }else{
    let resultData = await getTokens(req.body.requests);
    res.status(200).type('application/json').send(JSON.stringify(resultData));
  }
}

//Reads Firestore DB and gets user info to compare to supplied passwords
async function getTokens(dataArray){
  const db = new Firestore();
  let tokens = [];
  for(let i = 0; i < dataArray.length; ++i){
    let username = dataArray[i].username;
    let password = dataArray[i].password;
    const accountRef = db.collection('eos-accounts').doc(username);
    const doc = await accountRef.get();
    if (doc.exists) {
      let userData = doc.data();
      if(userData.pw == password){
        let token = createToken(username);
        token.user = username;
        tokens.push(token);
      }
    }
  }
  return {"tokens":tokens};
}

//Creates a JWT using HMAC-SHA256 signing
function createToken(username){
  const header = {
    "alg": "HS256",
    "typ": "JWT"
  };
  let payload = {
    "sub": username,
    "iss": "gcp",
    "aud": "eos"
  };

  let headerB64 = (Buffer.from(JSON.stringify(header))).toString('base64url');
  let payloadB64 = (Buffer.from(JSON.stringify(payload))).toString('base64url');
  let tokenEncode = headerB64+'.'+payloadB64;
  let sig = crypto.createHmac('sha256', process.env.SECRET).update(tokenEncode).digest('base64url');
  let token = tokenEncode+'.'+sig;
  let tokenResponse = {
    "id_token":token,
    "token_type": "Bearer",
  };
  return tokenResponse;
}