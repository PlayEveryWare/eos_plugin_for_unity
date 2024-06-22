EOS Demo Naming conventions:

Classes with a leading 'UI' are classes that hold or do UI related things
Classes ending with 'Menu' are the main classes that hold the sample's UI code
Classes ending with 'Entry' are UI list entries. 
Classes ending with 'Manager' & 'Service' _both_ handle interactions with the EOS SDK and are (potentially) shared across samples (efforts are underway to transition each 'Manager' class to be a 'Service' class, as the term is better representative of the functionality contained within as providing access to each "Service" made available via the EOS SDK).

For example: UIKeyboard is a script that deals with the UI keyboard
			 Peer2PeerManager handles the EOS side of the Peer 2 Peer demo scene