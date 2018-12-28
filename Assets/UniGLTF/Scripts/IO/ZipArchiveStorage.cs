using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.IO.Compression;
using System.Runtime.InteropServices;

/// <summary>
/// https://en.wikipedia.org/wiki/Zip_(file_format)
/// </summary>
namespace UniGLTF.Zip
{

    enum CompressionMethod : ushort
    {
        Stored = 0, // The file is stored (no compression)
        Shrink = 1, // The file is Shrunk
        Reduced1 = 2, // The file is Reduced with compression factor 1
        Reduced2 = 3, // The file is Reduced with compression factor 2
        Reduced3 = 4, // The file is Reduced with compression factor 3
        Reduced4 = 5, // The file is Reduced with compression factor 4
        Imploded = 6, // The file is Imploded
        Reserved = 7, // Reserved for Tokenizing compression algorithm
        Deflated = 8, // The file is Deflated
    }

    class ZipParseException : Exception
    {
        public ZipParseException(string msg) : base(msg)
        { }
    }

    class EOCD
    {
        public ushort NumberOfThisDisk;
        public ushort DiskWhereCentralDirectoryStarts;
        public ushort NumberOfCentralDirectoryRecordsOnThisDisk;
        public ushort TotalNumberOfCentralDirectoryRecords;
        public int SizeOfCentralDirectoryBytes;
        public int OffsetOfStartOfCentralDirectory;
        public string Comment;

        public override string ToString()
        {
            return string.Format("<EOCD records: {0}, offset: {1}, '{2}'>",
                NumberOfCentralDirectoryRecordsOnThisDisk,
                OffsetOfStartOfCentralDirectory,
                Comment
                );
        }

        static int FindEOCD(byte[] bytes)
        {
            for (int i = bytes.Length - 22; i >= 0; --i)
            {
                if (bytes[i] == 0x50
                    && bytes[i + 1] == 0x4b
                    && bytes[i + 2] == 0x05
                    && bytes[i + 3] == 0x06)
                {
                    return i;
                }
            }

            throw new ZipParseException("EOCD is not found");
        }

        public static EOCD Parse(Byte[] bytes)
        {
            var pos = FindEOCD(bytes);
            using (var ms = new MemoryStream(bytes, pos, bytes.Length - pos, false))
            using (var r = new BinaryReader(ms))
            {
                var sig = r.ReadInt32();
                if (sig != 0x06054b50) throw new ZipParseException("invalid eocd signature: " + sig);

                var eocd = new EOCD
                {
                    NumberOfThisDisk = r.ReadUInt16(),
                    DiskWhereCentralDirectoryStarts = r.ReadUInt16(),
                    NumberOfCentralDirectoryRecordsOnThisDisk = r.ReadUInt16(),
                    TotalNumberOfCentralDirectoryRecords = r.ReadUInt16(),
                    SizeOfCentralDirectoryBytes = r.ReadInt32(),
                    OffsetOfStartOfCentralDirectory = r.ReadInt32(),
                };

                var commentLength = r.ReadUInt16();
                var commentBytes = r.ReadBytes(commentLength);
                eocd.Comment = Encoding.ASCII.GetString(commentBytes);

                return eocd;
            }
        }
    }

    abstract class CommonHeader
    {
        public Encoding Encoding = Encoding.UTF8;
        public Byte[] Bytes;
        public int Offset;
        public abstract int Signature
        {
            get;
        }
        protected CommonHeader(Byte[] bytes, int offset)
        {
            var sig = BitConverter.ToInt32(bytes, offset);
            if (sig != Signature)
            {
                throw new ZipParseException("invalid central directory file signature: " + sig);
            }
            Bytes = bytes;
            Offset = offset;

            var start = offset + 4;
            using (var ms = new MemoryStream(bytes, start, bytes.Length - start, false))
            using (var r = new BinaryReader(ms))
            {
                ReadBefore(r);
                Read(r);
                ReadAfter(r);
            }
        }

        public UInt16 VersionNeededToExtract;
        public UInt16 GeneralPurposeBitFlag;
        public CompressionMethod CompressionMethod;
        public UInt16 FileLastModificationTime;
        public UInt16 FileLastModificationDate;
        public Int32 CRC32;
        public Int32 CompressedSize;
        public Int32 UncompressedSize;
        public UInt16 FileNameLength;
        public UInt16 ExtraFieldLength;

        public abstract int FixedFieldLength
        {
            get;
        }

        public abstract int Length
        {
            get;
        }

        public string FileName
        {
            get
            {
                return Encoding.GetString(Bytes,
                    Offset + FixedFieldLength,
                    FileNameLength);
            }
        }

        public ArraySegment<Byte> ExtraField
        {
            get
            {
                return new ArraySegment<byte>(Bytes,
                    Offset + FixedFieldLength + FileNameLength,
                    ExtraFieldLength);
            }
        }

        public override string ToString()
        {
            return string.Format("<file {0}({1}/{2} {3})>",
                FileName,
                CompressedSize,
                UncompressedSize,
                CompressionMethod
                );
        }

        public abstract void ReadBefore(BinaryReader r);

        public void Read(BinaryReader r)
        {
            VersionNeededToExtract = r.ReadUInt16();
            GeneralPurposeBitFlag = r.ReadUInt16();
            CompressionMethod = (CompressionMethod)r.ReadUInt16();
            FileLastModificationTime = r.ReadUInt16();
            FileLastModificationDate = r.ReadUInt16();
            CRC32 = r.ReadInt32();
            CompressedSize = r.ReadInt32();
            UncompressedSize = r.ReadInt32();
            FileNameLength = r.ReadUInt16();
            ExtraFieldLength = r.ReadUInt16();
        }

        public abstract void ReadAfter(BinaryReader r);
    }

    class CentralDirectoryFileHeader : CommonHeader
    {
        public override int Signature
        {
            get
            {
                return 0x02014b50;
            }
        }

        public CentralDirectoryFileHeader(Byte[] bytes, int offset) : base(bytes, offset) { }

        public UInt16 VersionMadeBy;
        public UInt16 FileCommentLength;
        public UInt16 DiskNumberWhereFileStarts;
        public UInt16 InternalFileAttributes;
        public Int32 ExternalFileAttributes;
        public Int32 RelativeOffsetOfLocalFileHeader;

        public override int FixedFieldLength
        {
            get
            {
                return 46;
            }
        }

        public string FileComment
        {
            get
            {
                return Encoding.GetString(Bytes,
                    Offset + 46 + FileNameLength + ExtraFieldLength,
                    FileCommentLength);
            }
        }

        public override int Length
        {
            get
            {
                return FixedFieldLength + FileNameLength + ExtraFieldLength + FileCommentLength;
            }
        }

        public override void ReadBefore(BinaryReader r)
        {
            VersionMadeBy = r.ReadUInt16();
        }

        public override void ReadAfter(BinaryReader r)
        {
            FileCommentLength = r.ReadUInt16();
            DiskNumberWhereFileStarts = r.ReadUInt16();
            InternalFileAttributes = r.ReadUInt16();
            ExternalFileAttributes = r.ReadInt32();
            RelativeOffsetOfLocalFileHeader = r.ReadInt32();
        }
    }

    class LocalFileHeader : CommonHeader
    {
        public override int FixedFieldLength
        {
            get
            {
                return 30;
            }
        }

        public override int Signature
        {
            get
            {
                return 0x04034b50;
            }
        }

        public override int Length
        {
            get
            {
                return FixedFieldLength + FileNameLength + ExtraFieldLength;
            }
        }

        public LocalFileHeader(Byte[] bytes, int offset) : base(bytes, offset)
        {
        }

        public override void ReadBefore(BinaryReader r)
        {
        }

        public override void ReadAfter(BinaryReader r)
        {
        }
    }

    class ZipArchiveStorage : IStorage
    {
        public override string ToString()
        {
            return string.Format("<ZIPArchive\n{0}>", String.Join("", Entries.Select(x => x.ToString() + "\n").ToArray()));
        }

        public List<CentralDirectoryFileHeader> Entries = new List<CentralDirectoryFileHeader>();

        public static ZipArchiveStorage Parse(byte[] bytes)
        {
            var eocd = EOCD.Parse(bytes);
            var archive = new ZipArchiveStorage();

            var pos = eocd.OffsetOfStartOfCentralDirectory;
            for (int i = 0; i < eocd.NumberOfCentralDirectoryRecordsOnThisDisk; ++i)
            {
                var file = new CentralDirectoryFileHeader(bytes, pos);
                archive.Entries.Add(file);
                pos += file.Length;
            }

            return archive;
        }

        public Byte[] Extract(CentralDirectoryFileHeader header)
        {
            var local = new LocalFileHeader(header.Bytes, header.RelativeOffsetOfLocalFileHeader);
            var pos = local.Offset + local.Length;

            var dst = new Byte[local.UncompressedSize];

#if true
            using (var s = new MemoryStream(header.Bytes, pos, local.CompressedSize, false))
            using (var deflateStream = new DeflateStream(s, CompressionMode.Decompress))
            {
                int dst_pos = 0;
                for (int remain = dst.Length; remain > 0;)
                {
                    var readSize = deflateStream.Read(dst, dst_pos, remain);
                    dst_pos += readSize;
                    remain -= readSize;
                }
            }
#else
            var size=RawInflate.RawInflateImport.RawInflate(dst, 0, dst.Length,
                header.Bytes, pos, header.CompressedSize);
#endif

            return dst;
        }

        public string ExtractToString(CentralDirectoryFileHeader header, Encoding encoding)
        {
            var local = new LocalFileHeader(header.Bytes, header.RelativeOffsetOfLocalFileHeader);
            var pos = local.Offset + local.Length;

            using (var s = new MemoryStream(header.Bytes, pos, local.CompressedSize, false))
            using (var deflateStream = new DeflateStream(s, CompressionMode.Decompress))
            using (var r = new StreamReader(deflateStream, encoding))
            {
                return r.ReadToEnd();
            }
        }

        public ArraySegment<byte> Get(string url)
        {
            var found = Entries.FirstOrDefault(x => x.FileName == url);
            if (found == null)
            {
                throw new FileNotFoundException("[ZipArchive]" + url);
            }

            switch (found.CompressionMethod)
            {
                case CompressionMethod.Deflated:
                    return new ArraySegment<byte>(Extract(found));

                case CompressionMethod.Stored:
                    return new ArraySegment<byte>(found.Bytes, found.RelativeOffsetOfLocalFileHeader, found.CompressedSize);
            }

            throw new NotImplementedException(found.CompressionMethod.ToString());
        }

        public string GetPath(string url)
        {
            return null;
        }
    }
}
