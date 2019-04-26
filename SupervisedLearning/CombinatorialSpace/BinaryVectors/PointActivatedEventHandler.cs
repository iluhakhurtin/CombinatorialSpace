using System;
using System.Collections.Generic;
using System.Text;

namespace CombinatorialSpace.BinaryVectors
{
    public delegate void PointActivatedEventHandler(object sender, PointActivatedEventArgs e);

    public class PointActivatedEventArgs: EventArgs
    {
        public int OutputBitIndex { get; private set; }

        public PointActivatedEventArgs(int outputBitIndex)
        {
            this.OutputBitIndex = outputBitIndex;
        }
    }
}
