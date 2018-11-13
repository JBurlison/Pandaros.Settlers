using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Pandaros.Settlers.Buildings.NBTReader.Nbt;

namespace Pandaros.Settlers.Buildings.NBTReader.Core
{

    public class NBTFile
    {
        private string _filename;

        public NBTFile (string path)
        {
            _filename = path;
        }

        public string FileName
        {
            get { return _filename; }
            protected set { _filename = value; }
        }

        public bool Exists ()
        {
            return File.Exists(_filename);
        }

        public void Delete ()
        {
            File.Delete(_filename);
        }

        public int GetModifiedTime ()
        {
            return Timestamp(File.GetLastWriteTime(_filename));
        }

        public virtual Stream GetDataInputStream()
        {
            using (FileStream fstr = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                long length = fstr.Seek(0, SeekOrigin.End);
                fstr.Seek(0, SeekOrigin.Begin);

                byte[] data = new byte[length];
                fstr.Read(data, 0, data.Length);

                return new MemoryStream(data);
            }
        }

        public virtual Stream GetDataOutputStream()
        {
           return new NBTBuffer(this);
        }

        class NBTBuffer : MemoryStream
        {
            private NBTFile file;

            public NBTBuffer (NBTFile c)
                : base(8096)
            {
                this.file = c;
            }

            public override void Close()
            {
                try
                {
                    using (Stream fstr = new FileStream(file._filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    {
                        try
                        {
                            fstr.Write(GetBuffer(), 0, (int)Length);
                        }
                        catch (Exception ex)
                        {
                            PandaLogger.LogError(ex, "Failed to write out NBT data stream.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    PandaLogger.LogError(ex, "Failed to open NBT data stream for output.");
                }
            }
        }

        private int Timestamp (DateTime time)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (int)((time - epoch).Ticks / (10000L * 1000L));
        }
    }
}
