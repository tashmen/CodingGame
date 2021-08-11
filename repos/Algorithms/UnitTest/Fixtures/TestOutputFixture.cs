﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace UnitTest.Fixtures
{
    public class TestOutputFixture : TextWriter
    {
        ITestOutputHelper _output;
        public TestOutputFixture(ITestOutputHelper output)
        {
            _output = output;
        }
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
        public override void WriteLine(string message)
        {
            _output.WriteLine(message);
        }
        public override void WriteLine(string format, params object[] args)
        {
            _output.WriteLine(format, args);
        }
    }
}
