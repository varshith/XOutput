﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using XOutput.Api.Message;

namespace XOutput.Api.Serialization.Tests
{
    [TestClass()]
    public class MessageReaderTests
    {
        private MessageReader reader = new MessageReader();

        [TestMethod]
        public void InputDataTest()
        {
            string input = "{\"Type\":\"InputData\",\"Data\":[{\"InputType\":\"test\",\"Value\":0.5}]}";
            var message = reader.ReadMessage(input) as InputDataMessage;
            Assert.IsNotNull(message);
            Assert.AreEqual("InputData", message.Type);
            Assert.IsNotNull(message.Data);
            Assert.IsTrue(message.Data.Count == 1);
            Assert.AreEqual("test", message.Data[0].InputType);
            Assert.AreEqual(0.5, message.Data[0].Value, 0.01);
        }

        [TestMethod]
        public void DebugTest()
        {
            string input = "{\"Type\":\"Debug\",\"Data\":\"test\"}";
            var message = reader.ReadMessage(input) as DebugMessage;
            Assert.IsNotNull(message);
            Assert.AreEqual("Debug", message.Type);
            Assert.AreEqual("test", message.Data);
        }

        [TestMethod]
        public void UnknownMessageTest()
        {
            string input = "{\"Type\":\"test\"}";
            var message = reader.ReadMessage(input) as MessageBase;
            Assert.IsNotNull(message);
            Assert.AreEqual("test", message.Type);
        }
    }
}