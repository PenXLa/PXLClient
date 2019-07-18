using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLClient.netKernel {
    public class Package {
        public byte[] data;

        class StringPos {
            public string str;
            public int pos;
        }
        List<byte> bin;
        List<StringPos> stringPool;

        //用作接受包的构造方法
        public Package(byte[] data) {
            this.data = data;
        }

        //用作创建包的构造方法,所以实例化用于写包的两个类
        public Package(int type) {
            bin = new List<byte>();
            stringPool = new List<StringPos>();
            putInt(type);
        }

        public int getType() {
            return (int)NetUtils.bin2integer(data, 0, ServerConnection.POINTER_SIZE);
        }
        public byte readByte(int offset) {
            return (byte)NetUtils.bin2integer(data, offset, sizeof(byte));
        }
        public short readShort(int offset) {
            return (short)NetUtils.bin2integer(data, offset, sizeof(short));
        }
        public int readInt(int offset) {
            return (int)NetUtils.bin2integer(data, offset, sizeof(int));
        }
        public long readLong(int offset) {
            return NetUtils.bin2integer(data, offset, sizeof(long));
        }
        public float readFloat(int offset) {
            return NetUtils.b2f(data, offset);
        }
        public double readDoube(int offset) {
            return NetUtils.b2d(data, offset);
        }
        public String readString(int off) {
            int pos = (int)NetUtils.bin2integer(data, off, ServerConnection.POINTER_SIZE), len = (int)NetUtils.bin2integer(data, off + ServerConnection.POINTER_SIZE, ServerConnection.POINTER_SIZE);
            return Encoding.UTF8.GetString(data, pos, len);
        }
        public string[] readStrings(int offset) {
            string[] strs = new string[readInt(offset)];
            for (int i = 0; i < strs.Length; ++i)
                strs[i] = readString(offset + ServerConnection.POINTER_SIZE + i * 2 * ServerConnection.POINTER_SIZE);
            return strs;
        }


        public void putByte(byte val) {
            byte[] data = NetUtils.integer2bin(val, sizeof(byte));
            foreach (byte b in data) bin.Add(b);
        }
        public void putShort(short val) {
            byte[] data = NetUtils.integer2bin(val, sizeof(short));
            foreach (byte b in data) bin.Add(b);
        }
        public void putInt(int val) {
            byte[] data = NetUtils.integer2bin(val, sizeof(int));
            foreach (byte b in data) bin.Add(b);
        }
        public void putLong(long val) {
            byte[] data = NetUtils.integer2bin(val, sizeof(long));
            foreach (byte b in data) bin.Add(b);
        }
        public void putFloat(float val) {
            byte[] data = NetUtils.f2b(val);
            foreach (byte b in data) bin.Add(b);
        }
        public void putDouble(double val) {
            byte[] data = NetUtils.d2b(val);
            foreach (byte b in data) bin.Add(b);
        }

        public void putString(string str) {
            StringPos sp = new StringPos();
            sp.pos = bin.Count;
            sp.str = str;
            for (int i = 0; i < ServerConnection.POINTER_SIZE * 2; ++i) bin.Add((byte)0);//为 字符串起始位置 和 字符串长度 占个位
            stringPool.Add(sp);
        }
        public void putStrings(string[] strs) {
            putInt(strs.Length);
            foreach (string s in strs)
                putString(s);
        }

        public void pack() {
            int size = 0;
            foreach (StringPos sp in stringPool) size += Encoding.UTF8.GetByteCount(sp.str);
            byte[] data = new byte[bin.Count + size];
            byte[] strdata = null, posdata = null, lendata = null;
            int pos = 0;
            for (; pos < bin.Count; ++pos) data[pos] = bin[pos];//基本数据

            foreach (StringPos sp in stringPool) {
                strdata = Encoding.UTF8.GetBytes(sp.str);
                posdata = NetUtils.integer2bin(pos, ServerConnection.POINTER_SIZE);
                lendata = NetUtils.integer2bin(strdata.Length, ServerConnection.POINTER_SIZE);
                for (int i = 0; i < ServerConnection.POINTER_SIZE; ++i) data[sp.pos + i] = posdata[i];//修改刚开始占了位置的str起始位置
                for (int i = 0; i < ServerConnection.POINTER_SIZE; ++i) data[sp.pos + ServerConnection.POINTER_SIZE + i] = lendata[i];//修改刚开始占了位置的str长度
                for (int i = 0; i < strdata.Length; ++i) data[pos + i] = strdata[i];
                pos += strdata.Length;
            }
            this.data = data;
        }

        public void clear() {
            bin.Clear();
            stringPool.Clear();
            data = null;
        }
    }
}
