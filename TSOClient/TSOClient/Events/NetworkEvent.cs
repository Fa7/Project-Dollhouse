﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSOClient.Events
{
    public class NetworkEvent : EventObject
    {
        public NetworkEvent(EventCodes ECode)
            : base(ECode)
        {

        }
    }
}
