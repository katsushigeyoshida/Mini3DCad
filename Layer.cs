using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mini3DCad
{
    public class Layer
    {
        public int mLayerSize = 8;          //  レイヤーbyteサイズ (size x 8)
        public byte[] mLayerBit;            //  レイヤーBit
        public Dictionary<int, string> mLayerList;

        public Layer(int size)
        {
            mLayerSize = size;
            mLayerBit = new byte[size / 8];
            mLayerList = new Dictionary<int, string>();
        }
        public string binary2HexString(int start = 0, int size = 0)
        {
            string buf = "";
            if (size == 0) size = mLayerBit.Length;
            for (int i = start; i < start + size && i < mLayerBit.Length; i++) {
                buf += string.Format("{0:X2} ", mLayerBit[i]);
            }
            return buf;
        }
        public int add(string name)
        {
            if (mLayerList.ContainsValue(name)) {
                var val = mLayerList.FirstOrDefault(x => x.Value == name);
                return val.Key;
            } else {
                for (int i = 0; i < mLayerSize * 8; i++) {
                    if (!mLayerList.ContainsKey(i)) {
                        mLayerList.Add(i, name);
                        bitOn(i);
                        return i;
                    }
                }
            }
            return -1;
        }
        public int upDateList(int[] keyList)
        {
            Dictionary<int, string> layerList = new Dictionary<int, string>();
            Array.Fill<byte>(mLayerBit, 0);
            foreach (int n in keyList) {
                if (!layerList.ContainsKey(n)) {
                    layerList.Add(n, mLayerList[n]);
                    bitOn(n);
                }
            }
            mLayerList = layerList;
            return mLayerList.Count;
        }
        public bool containsKey(int n)
        {
            return mLayerList.ContainsKey(n);
        }
        public string gatLayerName(int n)
        {
            if (mLayerList.ContainsKey(n))
                return mLayerList[n];
            else
                return "";
        }
        public void bitOn(int n)
        {
            byte b = 1;
            b <<= n % 8;
            mLayerBit[n / 8] |= b;
        }
        public void bitOff(int n)
        {
            byte b = 1;
            b <<= n % 8;
            mLayerBit[n / 8] &= (byte)~b;
        }
    }
}
