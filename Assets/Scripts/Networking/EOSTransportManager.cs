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

//#define EOS_TRANSPORTMANAGER_DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

#if COM_UNITY_MODULE_NETCODE
using Unity.Netcode;
#endif

using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using System.Text.RegularExpressions;

using PlayEveryWare.EpicOnlineServices.Samples;


namespace PlayEveryWare.EpicOnlineServices.Samples.Network
{
    public class EOSTransportManager : IEOSSubManager
    {

        // Maximum number of fragments to split into, due to storing the fragment number in a ushort without using the highest bit
        private const ushort MaxFragments = short.MaxValue + 1;
        // The header for each fragment contains 4 bytes: 2 bytes for the message index (ushort), and 2 bytes for the fragment number and end flag (ushort with the high bit reserved)
        private const ushort FragmentHeaderSize = 4;

        /// <summary>
        /// Maximum packet byte length
        /// </summary>
        /// <value><c>1170</c> due to DTLS/SCTP/UDP packet overhead.</value>
        public const int MaxPacketSize = P2PInterface.MaxPacketSize;

        /// <summary>
        /// Maximum concurrent connections with any individual remote peer.
        /// </summary>
        /// <value><c>32</c></value>
        public const int MaxConnections = P2PInterface.MaxConnections;

        // Connection data, socket name must be unique within an individual remote peer.
        public class Connection : IEquatable<Connection>
        {
            /// <summary>
            /// The ID of the socket for this Connection.
            /// </summary>
            public SocketId SocketId = new SocketId();

            /// <summary>
            /// The name of the socket used by this Connection.
            /// </summary>
            public string SocketName { get => SocketId.SocketName; set => SocketId.SocketName = value; }

            /// <summary>
            /// If the outgoing (local) side of the connection has been opened.
            /// </summary>
            public bool OpenedOutgoing = false;

            /// <summary>
            /// If the incoming (remote) side of the connection has been opened.
            /// </summary>
            public bool OpenedIncoming = false;

            /// <summary>
            /// If we are waiting on the remote side of the connection to confirm.
            /// </summary>
            public bool IsPendingOutgoing { get => IsValid && (OpenedOutgoing && !OpenedIncoming); }
            /// <summary>
            /// If the remote side of the connection is awaiting a connection accept response.
            /// </summary>
            public bool IsPendingIncoming { get => IsValid && (!OpenedOutgoing && OpenedIncoming); }

            /// <summary>
            /// If the connection has been opened on at least one end (local or remote).
            /// </summary>
            public bool IsHalfOpened { get => IsValid && (OpenedOutgoing || OpenedIncoming); }
            /// <summary>
            /// If the connection has been opened on both the local and remote ends.
            /// </summary>
            public bool IsFullyOpened { get => IsValid && (OpenedOutgoing && OpenedIncoming); }

            /// <summary>
            /// Has the user been given the chance to handle the connection open event?
            /// </summary>
            public bool ConnectionOpenedHandled = false;

            /// <summary>
            /// Has the user been given the chance to handle the connection closed event?
            /// </summary>
            public bool ConnectionClosedHandled = false;

            private ushort CurrentPacketIndex = 0;

            /// <summary>
            /// By design we don't re-use this Connection data structure after the connection lifecycle is complete.
            /// </summary>
            public bool IsValid = false;

            /// <summary>
            /// Creates a Connection with no initial information.
            /// </summary>
            public Connection() { }

            /// <summary>
            /// Creates a Connection with a given named socket.
            /// </summary>
            /// <param name="socketName">The name of the socket to use.</param>
            public Connection(string socketName) { SocketName = socketName; }

            /// <summary>
            /// Gets the ID to use for the next outgoing message on this connection.
            /// Note: The index is updated after each call, and it is expected that the value given will be used.
            /// </summary>
            /// <returns>The index to use for the next message.</returns>
            public ushort GetNextMessageIndex() { return CurrentPacketIndex++; }

            /// <summary>
            /// Gets the hash code for the socket.
            /// </summary>
            /// <returns>The hash code for this Connection's socket.</returns>
            public override int GetHashCode()
            {
                return SocketName.GetHashCode();
            }

            /// <summary>
            /// Checks if a given object is the same as this Connection.
            /// </summary>
            /// <param name="obj">The object to compare to.</param>
            /// <returns><c>true</c> if the objects match, <c>false</c> if not.</returns>
            public override bool Equals(object obj)
            {
                return Equals(obj as Connection);
            }

            /// <summary>
            /// Checks if the given socket name matches this Connection.
            /// </summary>
            /// <param name="socketName">The name of the socket to check.</param>
            /// <returns><c>true</c> if the socket name matches, <c>false</c> if not.</returns>
            public bool Equals(string socketName)
            {
                return SocketName == socketName;
            }

            /// <summary>
            /// Checks if a given Connection object is the same as this Connection.
            /// </summary>
            /// <param name="connection">The Connection to compare to.</param>
            /// <returns><c>true</c> if the Connections match, <c>false</c> if not.</returns>
            public bool Equals(Connection connection)
            {
                return SocketName == connection.SocketName;
            }

            /// <summary>
            /// Provides a JSON formatted debug string for this connection.
            /// </summary>
            /// <returns>The JSON formatted debug string containing the socket ID and socket name.</returns>
            public string DebugStringJSON()
            {
                return string.Format("{{\"SocketName\": {1}\"}}", SocketName);
            }
        }

        /// <summary>
        /// Called when a connection is originally requested by a remote peer (opened remotely but not yet locally).
        /// You may accept the connection by calling OpenConnection, or reject the connection by calling CloseConnection.
        /// </summary>
        /// <param name="remoteUserId">The id of the remote peer requesting the connection.</param>
        /// <param name="socketName">The name of the socket for the requested connection.</param>
        public delegate void OnIncomingConnectionRequestedCallback(ProductUserId remoteUserId, string socketName);

        /// <summary>
        /// Called immediately after a remote peer connection becomes fully opened (opened both locally and remotely).
        /// </summary>
        /// <param name="remoteUserId">The id of the remote peer that has been connected to.</param>
        /// <param name="socketName">The name of the socket the connection has opened on.</param>
        public delegate void OnConnectionOpenedCallback(ProductUserId remoteUserId, string socketName);

        /// <summary>
        /// Called immediately before a fully opened remote peer connection is closed (closed either locally or remotely).
        /// Guaranteed to be called only if OnConnectionOpenedCallback was attempted to be called earlier for any given connection.
        /// </summary>
        /// <param name="remoteUserId">The id of the remote peer to close the connection with.</param>
        /// <param name="socketName">The name of the socket to close the connection on.</param>
        public delegate void OnConnectionClosedCallback(ProductUserId remoteUserId, string socketName);

        /// <summary>
        /// Callback function for incoming connection requests.
        /// </summary>
        public OnIncomingConnectionRequestedCallback OnIncomingConnectionRequestedCb = null;

        /// <summary>
        /// Callback function for opening a connection
        /// </summary>
        public OnConnectionOpenedCallback OnConnectionOpenedCb = null;

        /// <summary>
        /// Callback function for closing a connection
        /// </summary>
        public OnConnectionClosedCallback OnConnectionClosedCb = null;

        /// <summary>
        /// Handle to the EOS P2P interface
        /// </summary>
        public P2PInterface P2PHandle;

        /// <summary>
        /// Cached value of the NAT type in use.
        /// </summary>
        public NATType NATType;

        /// <summary>
        /// The product id for the local user.
        /// </summary>
        public ProductUserId LocalUserId;

        /// <summary>
        /// The product id for the local user override.
        /// </summary>
        private ProductUserId LocalUserIdOverride = null;

        // Maps remote users to a list of all open connections with that user
        private Dictionary<ProductUserId, List<Connection>> Connections;

        // Maps a message index to the list of fragments received so far
        private Dictionary<ushort, SortedList<ushort, byte[]> > InProgressPackets;

        private bool IsInitialized = false;

        private static readonly byte[] ConnectionConfirmationPacket = Encoding.ASCII.GetBytes("READY");

        private const byte ConnectionConfirmationChannel = byte.MaxValue;

        [System.Diagnostics.Conditional("EOS_TRANSPORTMANAGER_DEBUG")]
        private void print(string msg)
        {
            Debug.Log(msg);
        }

        [System.Diagnostics.Conditional("EOS_TRANSPORTMANAGER_DEBUG")]
        private void printWarning(string msg)
        {
            Debug.LogWarning(msg);
        }

        [System.Diagnostics.Conditional("EOS_TRANSPORTMANAGER_DEBUG")]
        private void printError(string msg)
        {
            Debug.LogError(msg);
        }

        /// <summary>
        /// Gets a string containing debug information related to the state of the P2P manager.
        /// </summary>
        /// <returns>A JSON formatted string containing the local user id, NAT type, and any existing connections.</returns>
        public string GetDebugString(bool includeConnections = false)
        {
            if (IsInitialized == false)
            {
                return "{}";
            }
            else
            {
                string connectionString = includeConnections ? ConnectionsJSONFormatString() : "";
                return $"{{\"LocalUserId\": {LocalUserId}, \"NATType\": {NATType}{connectionString}}}";
            }
        }

        private string ConnectionsJSONFormatString()
        {
            string res = "";

            foreach (KeyValuePair<ProductUserId, List<Connection>> entry in Connections)
            {
                ProductUserId user = entry.Key;
                res += string.Format("{{\"RemoteUserId\": {0}, \"Connections\": [", user.ToString());
                for (int j = 0; j < entry.Value.Count; ++j)
                {
                    res += entry.Value[j].DebugStringJSON();

                    if (j + 1 < entry.Value.Count)
                        res += ", ";
                }
                res += "]}";
            }
            return string.Format(", \"Remote Users\": [{0}]", res);
        }

        /// <summary>
        /// Creates a new, uninitialized instance of EOSTransportManager.
        /// </summary>
        public EOSTransportManager()
        {
#if !COM_UNITY_MODULE_NETCODE
            Debug.LogError("EOSTransportManager: Netcode for GameObjects package not installed");
#endif

            Clear();
        }

        /// <summary>
        /// Creates a new, uninitialized instance of EOSTransportManager with an override user ID.
        /// </summary>
        public EOSTransportManager(ProductUserId overrideId)
        {
#if !COM_UNITY_MODULE_NETCODE
            Debug.LogError("EOSTransportManager: Netcode for GameObjects package not installed");
#endif

            Clear();
            LocalUserIdOverride = overrideId;
        }

        private void Clear()
        {
            P2PHandle = null;
            NATType = NATType.Unknown;
            LocalUserId = null;
            Connections = null;

            IsInitialized = false;
        }

#if UNITY_EDITOR
        void OnPlayModeChanged(UnityEditor.PlayModeStateChange modeChange)
        {
            if (modeChange == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                //prevent attempts to call native EOS code while exiting play mode, which crashes the editor
                P2PHandle = null;
                Shutdown();
            }
        }
#endif

        /// <summary>
        /// Initializes the EOS P2P Manager.
        /// </summary>
        /// <returns><c>true</c> if the initialization was successful, <c>false</c> if not.</returns>
        public bool Initialize()
        {
            if (IsInitialized)
            {
                printWarning("EOSTransportManager.Initialize: Already initialized - Shutting down EOSTransportManager first before proceeding.");
                Shutdown();
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeChanged;
#endif

            Debug.Assert(IsInitialized == false);
            print("EOSTransportManager.Initialize: Initializing EOSTransportManager...");
            bool result;

            if (EOSManager.Instance == null)
            {
                printError("EOSTransportManager.Initialize: Failed to initialize EOSTransportManager - Unable to get EOSManager singleton instance.");
                result = false;
            }
            else
            {
                P2PHandle = EOSManager.Instance.GetEOSP2PInterface();
                NATType = NATType.Unknown;
                if (LocalUserIdOverride?.IsValid() == true)
                {
                    LocalUserId = LocalUserIdOverride;
                }
                else
                {
                    LocalUserId = EOSManager.Instance.GetProductUserId();
                }
                Connections = new Dictionary<ProductUserId, List<Connection>>();
                InProgressPackets = new Dictionary<ushort, SortedList<ushort, byte[]>>();
            }

            if (P2PHandle == null)
            {
                printError("EOSTransportManager.Initialize: Failed to initialize EOSTransportManager - Unable to get EOS P2PInterface handle.");
                result = false;
            }
            else if (LocalUserId == null || LocalUserId.IsValid() == false)
            {
                printError("EOSTransportManager.Initialize: Failed to initialize EOSTransportManager - Invalid local ProductUserId.");
                result = false;
            }
            else
            {
                result = true;
            }

            if (result == false)
            {
                Clear();
            }
            else
            {
                IsInitialized = true;
                SubscribeToConnectionRequestNotifications();
                SubscribeToConnectionClosedNotifications();
                QueryNATType();
            }

            return result;
        }

        /// <summary>
        /// Shuts down the EOS P2P manager if initialized and active.
        /// </summary>
        public void Shutdown()
        {
            if (IsInitialized == false)
            {
                printWarning("EOSTransportManager.Shutdown: EOSTransportManager is already shutdown or was never initialized.");
                return;
            }

            print($"EOSTransportManager.Shutdown: Shutting down EOSTransportManager... | EOSTransportManager={GetDebugString()}");
            CloseAllConnections();
            UnsubscribeFromConnectionClosedNotifications();
            UnsubscribeFromConnectionRequestNotifications();
            Clear();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
#endif
        }

        //
        // Login / Logout Events (Start Here!)
        //

        /// <summary>
        /// To be called after the local user is logged into EOS.
        /// </summary>
        public void OnLoggedIn()
        {
            print($"EOSTransportManager.OnLoggedIn: Logged in with LocalUserId '{EOSManager.Instance.GetProductUserId()}' - Initializing EOSTransportManager.");
            Initialize();
        }

        /// <summary>
        /// To be called before the local user is logged out of EOS.
        /// </summary>
        public void OnLoggedOut()
        {
            print($"EOSTransportManager.OnLoggedOut: Logging out with LocalUserId '{EOSManager.Instance.GetProductUserId()}' - Shutting down EOSTransportManager.");
            Shutdown();
        }

        //
        // NAT Type Management
        //

        /// <summary>
        /// Refreshes our cached NATType (async), which can then be retrieved immediately using GetNATType.
        /// </summary>
        public void QueryNATType()
        {
            print("EOSTransportManager.QueryNATType: Querying our NAT type...");
            var options = new QueryNATTypeOptions();
            P2PHandle.QueryNATType(ref options, null, OnQueryNATTypeCompleted);
        }

        private void OnQueryNATTypeCompleted(ref OnQueryNATTypeCompleteInfo data)
        {
            if (data.ResultCode != Result.Success)
            {
                // Don't warn for certain errors if they're common
                if (data.ResultCode != Result.NoConnection)
                {
                    printWarning($"EOSTransportManager.OnQueryNATTypeCompleted: Error result, {data.ResultCode}");
                }
                return;
            }
            print($"EOSTransportManager.OnQueryNATTypeCompleted: Successfully retrieved NATType '{data.NATType}' (previous value was '{NATType}').");
            NATType = data.NATType;
        }

        /// <summary>
        /// Retrieves the cached NAT type
        /// </summary>
        /// <returns>The previously cached NAT type.</returns>
        public NATType GetNATType()
        {
            var options = new GetNATTypeOptions();
            Result result = P2PHandle.GetNATType(ref options, out NATType natType);

            if (result == Result.NotFound)
            {
                //Debug.LogWarningFormat( "EOSTransportManager.GetNATType: NATType not found." );
                return NATType.Unknown;
            }

            if (result != Result.Success)
            {
                printWarning($"EOSTransportManager.GetNATType: Error while retrieving NATType, {result}.");
                return NATType.Unknown;
            }
            print($"EOSTransportManager.GetNATType: Successfully retrieved NATType '{natType}'.");
            return natType;
        }

        //
        // Peer Connection Management
        //

        /// <summary>
        /// Checks if the given socket name is valid. 
        /// Socket names may only contain 1-32 alphanumeric characters.
        /// </summary>
        /// <param name="name">The socket name to check.</param>
        /// <returns><c>true</c> if the socket name is valid, <c>false</c> if not.</returns>
        public static bool IsValidSocketName(string name)
        {
            return (name != null)
                && (name.Length > 0)
                && (name.Length <= 32)
                && Regex.IsMatch(name, "^[a-zA-Z0-9]+$");
        }

        /// <summary>
        /// The total number of connections active.
        /// This includes pending connections (incoming or outgoing) and fully open connections.
        /// </summary>
        public int AllConnectionsCount { get => Connections.Values.Sum(connections => connections.Count); }
        /// <summary>
        /// The number of pending outgoing connections.
        /// </summary>
        public int PendingOutgoingConnectionsCount { get => Connections.Values.Sum(connections => connections.Where(connection => connection.IsPendingOutgoing).Count()); }
        /// <summary>
        /// The number of pending outgoing connections.
        /// </summary>
        public int PendingIncomingConnectionsCount { get => Connections.Values.Sum(connections => connections.Where(connection => connection.IsPendingIncoming).Count()); }
        /// <summary>
        /// The number of fully opened connections.
        /// </summary>
        public int FullyOpenConnectionsCount { get => Connections.Values.Sum(connections => connections.Where(connection => connection.IsFullyOpened).Count()); }

        /// <summary>
        /// Checks if a connection exists to a given remote user
        /// </summary>
        /// <param name="remoteUserId">The id of the remote user to check for.</param>
        /// <param name="socketName">The name of the socket to check for connections on.</param>
        /// <returns><c>true</c> if a connection exists, <c>false</c> if not.</returns>
        public bool HasConnection(ProductUserId remoteUserId, string socketName)
        {
            return TryGetConnection(remoteUserId, socketName, out _);
        }

        /// <summary>
        /// Finds an existing connection to a given remote user if it exists
        /// </summary>
        /// <param name="remoteUserId">The id of the remote user to check for.</param>
        /// <param name="socketName">The name of the socket to check for connections on.</param>
        /// <param name="connection">Will contain the connection if it exists, or will be set to null if no connection is found.</param>
        /// <returns><c>true</c> if a connection exists, <c>false</c> if not.</returns>
        public bool TryGetConnection(ProductUserId remoteUserId, string socketName, out Connection connection)
        {
            if (Connections.TryGetValue(remoteUserId, out List<Connection> connections))
                connection = connections.Find(x => x.SocketName == socketName);
            else
                connection = null;

            return (connection != null);
        }

        /// <summary>
        /// Opens (requests/accepts) a named socket connection with a remote peer.
        /// EOS supports a limited number of open connections with any individual remote peer (see <see cref="MaxConnections"/>).
        /// </summary>
        /// <param name="remoteUserId">The id of the remote peer to open a connection with.</param>
        /// <param name="socketName">The name of the socket to open the connection on.</param>
        /// <returns><c>true</c> if a connection was requested, a pending connection request was successfully accepted, or the connection has already been locally opened (this case will log a warning), otherwise <c>false</c>.</returns>
        public bool OpenConnection(ProductUserId remoteUserId, string socketName)
        {
            print($"EOSTransportManager.OpenConnection: Attempting to locally open (outgoing) socket connection named '{socketName}' with remote peer '{remoteUserId}'...");
            return Internal_OpenConnection(remoteUserId, socketName, true, out Connection _);
        }

        private bool Internal_OpenConnection(ProductUserId remoteUserId, string socketName, bool openOutgoing, out Connection connection)
        {
            connection = null;

            // EOSTransportManager is not initialized?
            if (IsInitialized == false)
            {
                printError("EOSTransportManager.Internal_OpenConnection: Failed to open remote peer connection - EOSTransportManager is uninitialized (has OnLoggedIn been called?).");
                return false;
            }

            // Socket name is invalid?
            if (IsValidSocketName(socketName) == false)
            {
                printError($"EOSTransportManager.Internal_OpenConnection: Failed to open remote peer connection - Socket name '{socketName}' is invalid (EOS socket names may only contain 1-32 alphanumeric characters).");
                return false;
            }

            // Get/add connection mappings for this remote peer
            List<Connection> connections;

            // Do we have any connections mapped for this remote peer?
            if (Connections.TryGetValue(remoteUserId, out List<Connection> foundConnections))
            {
                connections = foundConnections;

                // Max number of connections reached for this remote peer?
                if (connections.Count >= MaxConnections)
                {
                    printError($"EOSTransportManager.Internal_OpenConnection: Failed to open remote peer connection - Reached maximum number of open connections ({MaxConnections}) with the specified remote peer.");
                    return false;
                }
            }
            else
            {
                // Add connections map for this remote peer
                connections = new List<Connection>();
                Connections.Add(remoteUserId, connections);
            }

            // Get/add remote peer connection
            connection = connections.Find(x => x.SocketName == socketName);

            // We don't have a pre-existing connection?
            if (connection == null)
            {
                connection = new Connection(socketName);
                connections.Add(connection);
            }
            // We have a pre-existing connection.
            else
            {
                // Sanity check: We should not be re-using the Connection data structure after connection closure
                Debug.Assert(connection.ConnectionClosedHandled == false);

                // Already full opened?
                if (connection.IsFullyOpened)
                {
                    // Nothing left to do
                    printWarning($"EOSTransportManager.Internal_OpenConnection: Already have a fully opened socket connection named '{socketName}' with remote peer '{remoteUserId}'.");
                    return true;
                }

                // Trying to open in the outgoing direction?
                if (openOutgoing)
                {
                    // Already did?
                    if (connection.OpenedOutgoing)
                    {
                        // Nothing left to do
                        printWarning($"EOSTransportManager.Internal_OpenConnection: Already have a locally opened socket connection named '{socketName}' with remote peer '{remoteUserId}'. Now we're just awaiting a response to our connect request.");
                        return true;
                    }

                    // This pre-existing connection should be a pending incoming connection (ie. awaiting this outgoing open call)
                    Debug.Assert(connection.IsPendingIncoming);
                }
                // Trying to open in the incoming direction?
                else
                {
                    // Already did?
                    if (connection.OpenedIncoming)
                    {
                        // Nothing left to do
                        printWarning($"EOSTransportManager.Internal_OpenConnection: Already have a remotely opened socket connection named '{socketName}' with remote peer '{remoteUserId}'. Now we just need to respond to their connect request.");
                        return true;
                    }

                    // This pre-existing connection should be a pending outgoing connection (ie. awaiting this incoming open call)
                    Debug.Assert(connection.IsPendingOutgoing);
                }
            }

            // Trying to open in the outgoing direction?
            if (openOutgoing)
            {
                // Accept/request connection with remote peer on this socket name
                var options = new AcceptConnectionOptions()
                {
                    LocalUserId = LocalUserId,
                    RemoteUserId = remoteUserId,
                    SocketId = connection.SocketId,
                };

                // Note: By design P2PInterface.AcceptConnection performs an outgoing connection request if there isn't already a pending connection request to accept
                Result result = P2PHandle.AcceptConnection(ref options);
                if (result != Result.Success)
                {
                    printError($"EOSTransportManager.Internal_OpenConnection: Failed to open remote peer connection - P2PInterface.AcceptConnection error result '{result}'.");

                    // We just added this connection? (it would still be invalid at this point)
                    if (connection.IsValid == false)
                    {
                        // Undo adding it
                        connections.Remove(connection);
                    }
                    connection = null;

                    return false;
                }
            }

            // Validate connection
            connection.IsValid = true;

            // Trying to open in the outgoing direction?
            if (openOutgoing)
            {
                // Connection has now been opened from our local perspective (outgoing)
                connection.OpenedOutgoing = true;
                Debug.Assert(connection.IsPendingOutgoing || connection.IsFullyOpened);
            }
            // Trying to open in the incoming direction?
            else
            {
                // Connection has now been opened from their remote perspective (incoming)
                connection.OpenedIncoming = true;
                Debug.Assert(connection.IsPendingIncoming || connection.IsFullyOpened);
            }

            // Connection is now considered fully open?
            if (connection.IsFullyOpened)
            {
                // Send a confirmation packet to let the remote peer know we're fully connected and ready to send/receive application data
                SendPacket(remoteUserId, connection.SocketName, ConnectionConfirmationPacket, ConnectionConfirmationChannel, true);
            }

            // Handle connection open (user callback, etc.) if appropriate to do so
            TryHandleConnectionOpened(remoteUserId, socketName, connection);

            // Success
            return true;
        }

        /// <summary>
        /// Closes (cancels/rejects) a named socket connection with a remote peer.
        /// </summary>
        /// <param name="remoteUserId">The id of the remote peer to close connection with.</param>
        /// <param name="socketName">The name of the socket the exiting connection is on.</param>
        /// <param name="forceClose">If we should ignore any connection closure related errors (which will still be logged) and continue with connection cleanup locally (calling user callbacks, etc.)</param>
        /// <returns><c>true</c> if a matching remote peer connection was found and closed, <c>false</c> if not.</returns>
        public bool CloseConnection(ProductUserId remoteUserId, string socketName, bool forceClose = true)
        {
            print($"EOSTransportManager.CloseConnection: Attempting to close (cancel or reject) a socket connection named '{socketName}' with remote peer '{remoteUserId}'...");

            // EOSTransportManager is not initialized?
            if (IsInitialized == false)
            {
                printError("EOSTransportManager.CloseConnection: Failed to close remote peer connection - EOSTransportManager is uninitialized (has OnLoggedIn been called?).");
                return false;
            }

            if (remoteUserId == null)
            {
                printError("EOSTransportManager.CloseConnection: Failed to close remote peer connection - remoteUserId is null.");
                return false;
            }

            bool success;

            // Get remote peer connections
            if (Connections.TryGetValue(remoteUserId, out List<Connection> connections))
            {
                // Get remote peer connection
                Connection connection = connections.Find(x => x.SocketName == socketName);
                if (connection != null)
                {
                    var options = new CloseConnectionOptions()
                    {
                        LocalUserId = LocalUserId,
                        RemoteUserId = remoteUserId,
                        SocketId = connection.SocketId,
                    };

                    Result result = P2PHandle?.CloseConnection(ref options) ?? Result.NetworkDisconnected;
                    if (result != Result.Success)
                    {
                        printError($"EOSTransportManager.CloseConnection: Failed to close remote peer connection - P2PInterface.CloseConnection error result '{result}'.");
                        if (forceClose)
                        {
                            // Continue with local connection cleanup even though P2PInterface.CloseConnection failed
                            success = false;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        success = true;
                    }

                    // Handle connection closed (user callback, etc.) if appropriate to do so
                    if (forceClose)
                    {
                        //Flag connection as handled opened so it can be handled closed
                        connection.ConnectionOpenedHandled = true;
                    }
                    TryHandleConnectionClosed(remoteUserId, socketName, connection);

                    // Invalidate connection
                    connection.IsValid = false;

                    // Remove mapped connection
                    connections.Remove(connection);

                    // All mapped connections removed for this remote peer?
                    if (connections.Count <= 0)
                    {
                        // Remove remote peer from connections map
                        Connections.Remove(remoteUserId);
                    }

                    // Success
                    return success;
                }
            }

            success = false;
            printError($"EOSTransportManager.CloseConnection: Failed to close remote peer connection - Unable to find a socket connection named '{socketName}' with remote peer '{remoteUserId}'.");
            return false;
        }

        /// <summary>
        /// Closes all open connections.
        /// </summary>
        /// <param name="forceClose">If we should ignore any connection closure related errors (which will still be logged) and continue with connection cleanup locally (calling user callbacks, etc.)</param>
        /// <returns><c>true</c> if all connections were successfully closed, <c>false</c> if one or more connections failed to close.</returns>
        public bool CloseAllConnections(bool forceClose = true)
        {
            // EOSTransportManager is not initialized?
            if (IsInitialized == false)
            {
                printError($"EOSTransportManager.CloseAllConnections: Failed to close remote peer connections - EOSTransportManager is uninitialized (has OnLoggedIn been called?).");
                return false;
            }

            bool success = true;

            var remoteUserIdsCopy = Connections.Keys.ToList();
            foreach (var remoteUserId in remoteUserIdsCopy)
            {
                if (CloseAllConnectionsWithRemotePeer(remoteUserId, forceClose) == false)
                    success = false;
            }

            return success;
        }

        /// <summary>
        /// Closes all open connections with a given socket name.
        /// </summary>
        /// <param name="socketName">The name of the socket to close all connections on.</param>
        /// <param name="forceClose">If we should ignore any connection closure related errors (which will still be logged) and continue with connection cleanup locally (calling user callbacks, etc.)</param>
        /// <returns><c>true</c> if all connections with the given socket name were successfully closed, <c>false</c> if one or more connections failed to close.</returns>
        public bool CloseAllConnectionsWithSocketName(string socketName, bool forceClose = true)
        {
            // EOSTransportManager is not initialized?
            if (IsInitialized == false)
            {
                printError("EOSTransportManager.CloseAllConnectionsWithSocketName: Failed to close remote peer connections - EOSTransportManager is uninitialized (has OnLoggedIn been called?).");
                return false;
            }

            bool success = true;

            var remoteUserIdsCopy = Connections.Keys.ToList();
            foreach (var remoteUserId in remoteUserIdsCopy)
            {
                bool foundAndRemoved = Connections.TryGetValue(remoteUserId, out List<Connection> connections)
                                    && connections.Remove(new Connection(socketName));

                if (foundAndRemoved == false)
                    success = false;
            }

            return success;
        }

        // Returns true if all connections with the given remote peer were successfully closed, else false (one or more connections failed to close)
        /// <summary>
        /// Closes all open connections with a given remote peer.
        /// </summary>
        /// <param name="remoteUserId">The id of the remote user to close all connections with.</param>
        /// <param name="forceClose">If we should ignore any connection closure related errors (which will still be logged) and continue with connection cleanup locally (calling user callbacks, etc.)</param>
        /// <returns><c>true</c> if all connections with the given remote peer were successfully closed, <c>false</c> if one or more connections failed to close.</returns>
        public bool CloseAllConnectionsWithRemotePeer(ProductUserId remoteUserId, bool forceClose = true)
        {
            // EOSTransportManager is not initialized?
            if (IsInitialized == false)
            {
                printError("EOSTransportManager.CloseAllConnectionsWithRemotePeer: Failed to close remote peer connections - EOSTransportManager is uninitialized (has OnLoggedIn been called?).");
                return false;
            }

            bool success = true;

            if (Connections.TryGetValue(remoteUserId, out List<Connection> connections))
            {
                var connectionsCopy = new List<Connection>(connections);
                foreach (var connection in connectionsCopy)
                {
                    if (CloseConnection(remoteUserId, connection.SocketName, forceClose) == false)
                    {
                        success = false;
                    }
                }
            }

            return success;
        }

        private void TryHandleConnectionOpened(ProductUserId remoteUserId, string socketName, Connection connection)
        {
            // Connection is now fully open? (ie. Both sides have accepted the connection?)
            if (connection.IsFullyOpened)
            {
                // We should handle this event?
                if (OnConnectionOpenedCb != null && connection.ConnectionOpenedHandled == false)
                {
                    OnConnectionOpenedCb(remoteUserId, socketName);
                }

                // Handled in this context means we gave the user the opportunity to handle this event at the appropriate time
                connection.ConnectionOpenedHandled = true;
            }
        }

        private void TryHandleConnectionClosed(ProductUserId remoteUserId, string socketName, Connection connection)
        {
            // We should handle this event?
            if (OnConnectionClosedCb != null && connection.ConnectionOpenedHandled == true && connection.ConnectionClosedHandled == false)
            {
                OnConnectionClosedCb(remoteUserId, socketName);
            }

            // Handled in this context means we gave the user the opportunity to handle this event at the appropriate time
            connection.ConnectionClosedHandled = true;
        }

        //
        // Send / Receive Packets
        //

        /// <summary>
        /// Sends a packet to a given remote user on an open connection using the specified reliability.
        /// </summary>
        /// <param name="remoteUserId">The id of the remote user to send a packet to.</param>
        /// <param name="socketName">The name of the socket the connection is open on.</param>
        /// <param name="packet">The packet to be sent.</param>
        /// <param name="channel">Which channel the packet should be sent on.</param>
        /// <param name="allowDelayedDelivery">If <c>false</c> and there is not an existing connection to the peer, the data will be dropped.</param>
        /// <param name="reliability">Which level of reliability the packet should be sent with.</param>
        public void SendPacket(ProductUserId remoteUserId, string socketName, byte[] packet, byte channel = 0, bool allowDelayedDelivery = false, PacketReliability reliability = PacketReliability.ReliableOrdered)
        {
            if (remoteUserId.IsValid() == false)
            {
                printError($"EOSTransportManager.SendPacket: Invalid parameters, RemoteUserId '{remoteUserId}' is invalid.");
                return;
            }

            if (packet.Length <= 0)
            {
                printError("EOSTransportManager.SendPacket: Invalid parameters, packet is empty.");
                return;
            }
            if (packet.Length > (MaxPacketSize - FragmentHeaderSize) * MaxFragments)
            {
                printError($"EOSTransportManager.SendPacket: Fragmenting packet of size {packet.Length} would require more than {MaxFragments} fragments and cannot be sent.");
                return;
            }

            // Try to get the remote peer connection
            Connection connection = null;
            if (!Connections.TryGetValue(remoteUserId, out List<Connection> userConnections))
            {
                printError($"EOSTransportManager.SendPacket: Connection not found to remote user {remoteUserId}.");
                return;
            }
            connection = userConnections.Find(x => x.SocketName == socketName);
            if (connection == null)
            {
                printError($"EOSTransportManager.SendPacket: Connection not found on socket {socketName} to remote user {remoteUserId}.");
                return;
            }

            // Split the data into fragments if necessary. One extra is added to account for the remainder after filling as many packets as possible.
            int numFragments = (packet.Length / (MaxPacketSize - FragmentHeaderSize)) + 1;
            // The size of the remainder packet added to the count above
            int lastPacketSize = packet.Length - ((numFragments - 1) * (MaxPacketSize - FragmentHeaderSize));

            // If there is no remainder (data was evenly split into full packets), we don't need the extra packet
            if(lastPacketSize == 0)
            {
                --numFragments;
                lastPacketSize = MaxPacketSize-FragmentHeaderSize;
            }

            int currentOffset = 0;
            List<byte[]> messages = new List<byte[]>();

            ushort OutgoingFragmentedIndex = connection.GetNextMessageIndex();
            for (ushort i = 0; i < numFragments; ++i)
            {
                byte[] fragment = new byte[Mathf.Min(packet.Length + FragmentHeaderSize, MaxPacketSize)];
                // 4 Packet header: Bytes 1 and 2 hold the packed id, while 3 and 4 hold the fragment number, with the last bit used as a flag to mark the final fragment.
                fragment[0] = (byte)(OutgoingFragmentedIndex >> 8);
                fragment[1] = (byte)OutgoingFragmentedIndex;
                fragment[2] = (byte)(((i & short.MaxValue) >> 8) | (byte)(i == numFragments - 1 ? 128 : 0));
                fragment[3] = (byte)(i & short.MaxValue);

                int length = i == numFragments - 1 ? lastPacketSize : MaxPacketSize - FragmentHeaderSize;
                Array.Copy(packet, currentOffset,
                            fragment, FragmentHeaderSize, length);

                messages.Add(fragment);
                currentOffset += fragment.Length-FragmentHeaderSize;
            }

            for (int i = 0; i < numFragments; ++i)
            {
                byte[] fragment = messages[i];

                // Send Packet
                SocketId socketId = new SocketId()
                {
                    SocketName = socketName
                };

                SendPacketOptions options = new SendPacketOptions()
                {
                    LocalUserId = LocalUserId,
                    RemoteUserId = remoteUserId,
                    SocketId = socketId,
                    AllowDelayedDelivery = allowDelayedDelivery,
                    Channel = channel,
                    Reliability = reliability,
                    Data = new ArraySegment<byte>(fragment),
                };

                Result result = P2PHandle.SendPacket(ref options);
                if (result != Result.Success)
                {
                    printError($"EOSTransportManager.SendPacket: Unable to send {options.Data.Count} byte packet to RemoteUserId '{options.RemoteUserId}' - Error result, {result}.");
                    return;
                }

            }
#if EOS_P2PMANAGER_DEBUG
            Debug.LogFormat("EOSTransportManager.SendPacket: Successfully sent {0} byte packet to RemoteUserId '{1}'.", packet.Length, remoteUserId);
#endif
        }

        /// <summary>
        /// Tries to recieve a packet from any user.
        /// </summary>
        /// <param name="remoteUserId">The id of the packet's sender if one was recieved.</param>
        /// <param name="socketName">The socket the packet was sent on if one was recieved.</param>
        /// <param name="channel">The channel the packet was sent on if one was recieved.</param>
        /// <param name="packet">The packet data if one was recieved.</param>
        /// <returns><c>true</c> if a packet was recieved, <c>false</c> if not. In the latter case, all parameters will be set to <c>null</c> or <c>0</c></returns>
        public bool TryReceivePacket(out ProductUserId remoteUserId, out string socketName, out byte channel, out byte[] packet)
        {
            if (P2PHandle == null)
            {
                remoteUserId = null;
                socketName = null;
                channel = 0;
                packet = null;
                return false;
            }

            ReceivePacketOptions receivePacketOptions = new ReceivePacketOptions()
            {
                LocalUserId = LocalUserId,
                MaxDataSizeBytes = 4096,
                RequestedChannel = null
            };

            var getNextReceivedPacketSizeOptions = new GetNextReceivedPacketSizeOptions
            {
                LocalUserId = LocalUserId,
                RequestedChannel = null
            };
            P2PHandle.GetNextReceivedPacketSize(ref getNextReceivedPacketSizeOptions, out uint nextPacketSizeBytes);

            packet = new byte[nextPacketSizeBytes];
            var dataSegment = new ArraySegment<byte>(packet);

            //TODO: verify that this still works
            Result result = P2PHandle.ReceivePacket(ref receivePacketOptions, out remoteUserId, out SocketId socketId, out channel, dataSegment, out uint bytesWritten);
            socketName = socketId.SocketName;
            // No packets to be received?
            if (result == Result.NotFound)
            {
                remoteUserId = null;
                socketName = null;
                channel = 0;
                packet = null;
                return false;
            }

            // Packet is smaller than expected
            if (packet.Length < FragmentHeaderSize)
            {
                printError($"EOSTransportManager.TryReceivePacket: Received {packet.Length} byte packet. Should be at least {FragmentHeaderSize} bytes.");
                remoteUserId = null;
                socketName = null;
                channel = 0;
                packet = null;
                return false;
            }

            // Internal EOS P2P error?
            if (result != Result.Success)
            {
                printError($"EOSTransportManager.TryReceivePacket: Error result, {result}.");
                remoteUserId = null;
                socketName = null;
                channel = 0;
                packet = null;
                return false;
            }

            // Invalid user?
            if (remoteUserId.IsValid() == false)
            {
                printError($"EOSTransportManager.TryReceivePacket: Received {packet.Length} byte packet from invalid RemoteUserId '{remoteUserId}'.");
                remoteUserId = null;
                socketName = null;
                channel = 0;
                packet = null;
                return false;
            }


            ArraySegment<byte> header = new ArraySegment<byte>(packet, 0, FragmentHeaderSize);
            ArraySegment<byte> payload = new ArraySegment<byte>(packet, FragmentHeaderSize, packet.Length - FragmentHeaderSize);

            //TODO: verify that this still works
            // Combine the bytes from the header to form 2 shorts, one for the message index, and one for the fragment number/end flag
            ushort index = (ushort)((ushort)(header.Array[0] << 8) | header.Array[1]);
            ushort fragmentInfo = (ushort)((ushort)(header.Array[2] << 8) | header.Array[3]);
            // Extract the fragment number without the end flag on the high bit
            ushort fragmentPos = (ushort)(fragmentInfo & short.MaxValue);

            // Is this packet from a connection we recognize?
            if (TryGetConnection(remoteUserId, socketName, out Connection connection))
            {
                // Is this a connection confirmation packet?
                if (channel == ConnectionConfirmationChannel && payload.SequenceEqual(ConnectionConfirmationPacket))
                {
                    print($"EOSTransportManager.TryReceivePacket: Connection confirmation packet received for socket connection named '{socketName}' with remote peer '{remoteUserId}'.");

                    // We've been waiting for our connect request to be accepted on this connection?
                    if (connection.IsPendingOutgoing)
                    {
                        // They've accepted our connection, so we're no longer pending
                        print($"EOSTransportManager.TryReceivePacket: Attempting to remotely open (incoming) socket connection named '{socketName}' with remote peer '{remoteUserId}'...");
                        bool success = Internal_OpenConnection(remoteUserId, socketName, false, out _);

                        // Our connection should now be considered fully open
                        Debug.Assert(success && connection.IsFullyOpened);
                    }

                    // Discard this confirmation packet, we only return to the user application data packets
                    remoteUserId = null;
                    socketName = null;
                    channel = 0;
                    packet = null;
                    return false;
                }
            }
            else
            {
                printWarning($"EOSTransportManager.TryReceivePacket: Received a {packet.Length} byte packet from unknown RemoteUserId '{remoteUserId}', discarding packet.");
            }

            if (connection == null || connection.IsFullyOpened == false)
            {
                printWarning($"EOSTransportManager.TryReceivePacket: Received a {packet.Length} byte packet from RemoteUserId '{remoteUserId}', discarding packet.");

                // Discard this packet, we only return to the user packets from fully open peer connections
                remoteUserId = null;
                socketName = null;
                channel = 0;
                packet = null;
                return false;
            }

            if (!InProgressPackets.ContainsKey(index))
            {
                InProgressPackets[index] = new SortedList<ushort, byte[]>();
            }
            InProgressPackets[index].Add(fragmentPos,payload.ToArray());

            // If the end flag is not set, we are still waiting on other pieces
            if(((ushort)(fragmentInfo & (MaxFragments)) >> 15) != 1)
            {
                packet = null;
                return false;
            }

            // Combine the fragments and return the full packet
            int totalSize = 0;
            for (ushort i = 0; i < InProgressPackets[index].Count; ++i)
            {
                totalSize += InProgressPackets[index][i].Length;
            }

            
            byte[] finalPacket = new byte[totalSize];
            int offset = 0;
            for(ushort i = 0; i < InProgressPackets[index].Count; ++i)
            {
                Array.Copy(InProgressPackets[index][i], 0, finalPacket, offset, InProgressPackets[index][i].Length);
                offset += InProgressPackets[index][i].Length;
            }
            packet = finalPacket;

            // Clear the stored packet data and remove the index from the dictionary, as this packet does not need to be stored anymore.
            InProgressPackets[index].Clear();
            InProgressPackets.Remove(index);
            // Success
            print($"EOSTransportManager.TryReceivePacket: Successfully received {packet.Length} byte packet from RemoteUserId '{remoteUserId}'.");
            return true;
        }

        //
        // (Internal) P2P Connection Event Handling
        //

        private ulong ConnectionRequestNotificationsId = 0;

        private void SubscribeToConnectionRequestNotifications()
        {
            AddNotifyPeerConnectionRequestOptions options = new AddNotifyPeerConnectionRequestOptions()
            {
                LocalUserId = LocalUserId,
                SocketId = null, // Notify us about all connection requests to our local user regardless of socket name.
            };

            ConnectionRequestNotificationsId = P2PHandle.AddNotifyPeerConnectionRequest(ref options, null, OnConnectionRequestNotification);
        }
        private void UnsubscribeFromConnectionRequestNotifications()
        {
            P2PHandle?.RemoveNotifyPeerConnectionRequest(ConnectionRequestNotificationsId);
        }
        private void OnConnectionRequestNotification(ref OnIncomingConnectionRequestInfo data)
        {
            // Sanity check
            Debug.Assert(data.LocalUserId == LocalUserId);

            var socketName = data.SocketId?.SocketName;
            var remoteUserId = data.RemoteUserId;

            // Get/add the connection internally from the incoming direction
            print($"EOSTransportManager.OnConnectionRequestNotification: Attempting to remotely open (incoming) socket connection named '{socketName}' with remote peer '{remoteUserId}'...");
            bool success = Internal_OpenConnection(remoteUserId, socketName, false, out Connection connection);

            // Successfully found/added? And is now awaiting our connect accept response?
            if (success && connection.IsPendingIncoming)
            {
                // Give user the opportunity to accept/reject this connection
                if (OnIncomingConnectionRequestedCb != null)
                {
                    OnIncomingConnectionRequestedCb(remoteUserId, socketName);
                }
            }
            else
            {
                printError($"EOSTransportManager.OnConnectionRequestNotification: Failed to process connection request notification for socket connection named '{socketName}' with remote peer '{remoteUserId}'...");
            }
        }

        private ulong ConnectionClosedNotificationsId = 0;

        private void SubscribeToConnectionClosedNotifications()
        {
            AddNotifyPeerConnectionClosedOptions options = new AddNotifyPeerConnectionClosedOptions()
            {
                LocalUserId = LocalUserId,
                SocketId = null, // Notify us about all connection closures to our local user regardless of socket name.
            };

            ConnectionClosedNotificationsId = P2PHandle.AddNotifyPeerConnectionClosed(ref options, null, OnConnectionClosedNotification);
        }
        private void UnsubscribeFromConnectionClosedNotifications()
        {
            P2PHandle?.RemoveNotifyPeerConnectionClosed(ConnectionClosedNotificationsId);
        }
        private void OnConnectionClosedNotification(ref OnRemoteConnectionClosedInfo data)
        {
            // Sanity check
            Debug.Assert(data.LocalUserId == LocalUserId);

            var socketName = data.SocketId?.SocketName;
            var remoteUserId = data.RemoteUserId;

            // Force close (from incoming direction)
            CloseConnection(remoteUserId, socketName, true);
        }

        public bool StartHost()
        {
#if !COM_UNITY_MODULE_NETCODE
            Debug.LogError("EOSTransportManager (StartHost): Network for GameObjects package not installed");
            return false;
#else
            return NetworkManager.Singleton.StartHost();
#endif
        }

        public bool StartServer()
        {
#if !COM_UNITY_MODULE_NETCODE
            Debug.LogError("EOSTransportManager (StartServer): Network for GameObjects package not installed");
            return false;
#else
            return NetworkManager.Singleton.StartServer();
#endif
        }

        public bool StartClient()
        {
#if !COM_UNITY_MODULE_NETCODE
            Debug.LogError("EOSTransportManager (StartHost): Network for GameObjects package not installed");
            return false;
#else
            return NetworkManager.Singleton.StartClient();
#endif
        }

        public void Disconnect(bool discardMessageQueue = false)
        {
#if !COM_UNITY_MODULE_NETCODE
            Debug.LogError("EOSTransportManager (Shutdown): Network for GameObjects package not installed");
#else
            NetworkManager.Singleton?.Shutdown(discardMessageQueue);
#endif
        }
    }
}
