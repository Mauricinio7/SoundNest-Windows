﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundNest_Windows_Client.Notifications.ViewModels
{
    public class WelcomeViewModel
    {
        private readonly INotificationManager _manager;

        public string Title { get; set; }
        public string Message { get; set; }

        public WelcomeViewModel(INotificationManager manager)
        {
            _manager = manager;
        }

    }
}
