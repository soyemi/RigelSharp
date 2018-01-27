﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rigel
{
    public abstract class Singleton<T> where T: class,new ()
    {
        private static T s_instnace = null;
        public static T Instance {
            get
            {
                if (s_instnace == null)
                {
                    s_instnace = new T();
                }
                return s_instnace;
            }
        }
    }
}
