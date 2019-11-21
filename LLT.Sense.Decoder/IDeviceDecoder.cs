using System;
using System.Collections.Generic;

namespace LLT.Sense.Decoder
{
    public interface IDeviceDecoder
    {
        /// <summary>
        /// Decode the specified bytes.
        /// </summary>
        /// <returns>The decode.</returns>
        /// <param name="bytes">Bytes.</param>
        object Decode(byte[] bytes);
    }
}
