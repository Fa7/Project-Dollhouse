﻿/*The contents of this file are subject to the Mozilla Public License Version 1.1
(the "License"); you may not use this file except in compliance with the
License. You may obtain a copy of the License at http://www.mozilla.org/MPL/

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

The Original Code is the TSOClient.

The Initial Developer of the Original Code is
Mats 'Afr0' Vederhus. All Rights Reserved.

Contributor(s): ______________________________________.
*/

using System;
using System.Collections.Generic;
using System.Text;
using TSOClient.Network;
using TSOClient.VM;
using TSOClient.Lot;

namespace TSOClient
{
    /// <summary>
    /// A class representing the current player's account.
    /// Holds things such as the account's sims and client
    /// (used to communicate with the server).
    /// </summary>
    class PlayerAccount
    {
        public static List<Sim> Sims = new List<Sim>();
        
        public static NetworkClient Client;
        //The encryptionkey used to encrypt client's communcation. See UIPacketSenders.SendLoginRequest()
        public static byte[] EncKey = new byte[1];
        //The hash of the username and password. See UIPacketSenders.SendLoginRequest()
        public static byte[] Hash = new byte[1];
    }
}
