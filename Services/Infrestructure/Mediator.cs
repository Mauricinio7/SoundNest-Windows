﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Infrestructure
{
    public class Mediator
    {
        private static IDictionary<string, Action<object>> actions = new Dictionary<string, Action<object>>();

        public static void Register(string token, Action<object> callback)
        {
            if (!actions.ContainsKey(token))
            {
                actions[token] = callback;
            }
        }

        public static void Notify(string token, object args)
        {
            if (actions.ContainsKey(token))
            {
                actions[token](args);
            }
        }
    }
}
