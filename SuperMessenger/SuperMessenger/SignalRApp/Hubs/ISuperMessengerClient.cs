﻿using SuperMessenger.Models.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperMessenger.SignalRApp.Hubs
{
    public interface ISuperMessengerClient
    {
        Task SendMessage(Message message);
    }
}
