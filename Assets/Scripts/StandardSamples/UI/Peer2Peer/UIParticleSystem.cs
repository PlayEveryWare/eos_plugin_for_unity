/*
* Copyright (c) 2021 PlayEveryWare
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    using UnityEngine;

    public class UIParticleSystem : MonoBehaviour
    {
        public GameObject parent;


        public GameObject sprite;
        public float count;

        private float timer = 0;



        private void Start()
        {
            for (int i = 0; i < count; i++)
            {
                GameObject particle = Instantiate(sprite,
                    new Vector3(this.transform.position.x, this.transform.position.y, -3), Quaternion.identity,
                    this.transform);
                UIPeer2PeerParticleLifetimer timer = particle.AddComponent<UIPeer2PeerParticleLifetimer>();
                timer.direction = new Vector3(Random.Range(-100, 100), Random.Range(-100, 100),
                    Random.Range(-100, 100));
                particle.transform.Rotate(new Vector3(0, 0, Random.Range(360, 0)));


            }
        }

        void Update()
        {
            timer += Time.deltaTime;

            if (timer >= 1)
            {
                Destroy(parent);
            }
        }
    }
}
