using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking.Match;

namespace Assets.Scripts.ServerClient
{
    interface IServerClient
    {
        void OnDisable();

        void OnConnectionDropped(bool success, string extendedInfo);
    }
}
