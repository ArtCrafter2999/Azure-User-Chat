﻿using NetModelsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerClasses
{
    public class OperationFailureExeption : Exception
    {
        public RequestType? BusType { get; set; }
        public OperationFailureExeption(RequestType type, string message) : base(message) { BusType = type; }
        public OperationFailureExeption(string message) : base(message) { }
        public OperationFailureExeption() : base("не вдалося виконати цю операцію") { }
    }
}
