using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace 渣浪后黑
{
    public partial class fuckSina : Form
    {
        public fuckSina()
        {
            InitializeComponent();
        }
        Process changeToFlv = new Process();
        FlvParser _parser = null;
        
        private void startButton_Click(object sender, EventArgs e)
        {
            if (inputFileBox.Text=="")
            { return; }
            #region flv封装
            changeToFlv.StartInfo.Arguments = "-vcodec copy -acodec copy -y -i " + inputFileBox.Text + " -f flv " + inputFileBox.Text+"t";
            changeToFlv.Start();
            changeToFlv.WaitForExit();
            changeToFlv.Close();
            Thread.Sleep(2000);
            #endregion
            string _filename = inputFileBox.Text + "t";
            int n = 1;
            FileStream stream = new FileStream(_filename, FileMode.Open, FileAccess.Read);
            _parser = new FlvParser(stream, (tag) =>
            {
                string[] s = new string[]{
                            n++.ToString(),
                            tag.Type.ToString(),
                            ByteUtil.GetTime(tag.TimeStamp),
                            tag.Info1,
                            tag.Info2,
                            "0x" + tag.Offset.ToString("X8"),
                            tag.DataSize.ToString()
                        };
                FlvParser.TagType type = tag.Type;
                ListViewItem lvi = new ListViewItem(s);
                if (type == FlvParser.TagType.Audio)
                {
                    FlvParser.AudioTag atag = tag as FlvParser.AudioTag;
                }
                else if (type == FlvParser.TagType.Video)
                {
                    lvi.BackColor = Color.FromArgb(245, 239, 209);
                    FlvParser.VideoTag vtag = tag as FlvParser.VideoTag;
                }
                else if (type == FlvParser.TagType.Script)
                {
                    lvi.BackColor = Color.FromArgb(153, 217, 234);
                }
                //items.Add(lvi);
                return true;
            });
            stream.Close();
            long filesize = _parser.Length + 16;
            double duration;
            double rate = double.Parse(rateBox.Text);
            duration = filesize / 125.0 / rate; // * 8 / 1000 / rate;
            string offset = ((duration * 1000 - _parser.Duration) / 1000).ToString("0.000");
            string _path = outputFileBox.Text;
            new Thread(() =>
            {
                Stream src = new FileStream(_filename, FileMode.Open );
                string path = outputFileBox.Text;
                Stream dest = new FileStream(path, FileMode.Create);
                WriteHead(dest, filesize, duration, -1, -1, -1, 1.0, 0,
                    0, _parser.Tags.Count - 1, false);
                for (int i = 1; i < _parser.Tags.Count; i++)
                {
                    src.Seek(_parser.Tags[i].Offset - 11, SeekOrigin.Begin);
                    FlvParser.FlvTag tag = _parser.Tags[i];
                    byte[] bs = new byte[tag.DataSize + 11];
                    // 数据
                    src.Read(bs, 0, bs.Length);
                    dest.Write(bs, 0, bs.Length);
                    // prev tag size
                    src.Read(bs, 0, 4);
                    dest.Write(bs, 0, 4);

                    this.BeginInvoke(new MethodInvoker(() =>
                    {
                    }));
                }
                src.Close();
                src.Dispose();
                byte[] buffer = new byte[]{
                    0x09, 0, 0, 0x01, // 视频帧 1 字节
                    0, 0, 0, 0,       // 04h, timestamp & ex
                    0, 0, 0,          // stream id
                    0x17,             // InnerFrame, H.264
                    0, 0, 0, 0x0c     // 此帧长度 12 字节
                };
                uint dur = (uint)(duration * 1000);
                PutTime(buffer, 0x04, dur);
                dest.Write(buffer, 0, buffer.Length);

                dest.Flush();
                dest.Close();
                dest.Dispose();

                MessageBox.Show(this, "处理完毕，后黑了 " + offset + " 秒！",
                    "后黑", MessageBoxButtons.OK, MessageBoxIcon.Information);
                File.Delete(inputFileBox.Text + "t");
            }).Start();
        }

        private void fuckSina_Load(object sender, EventArgs e)
        {
            changeToFlv.StartInfo.FileName = "ffmpeg.exe";
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        #region - Write 函数 -
        private int PutInt(byte[] dest, int pos, int val, int length)
        {
            if (length <= 0)
                return pos;
            for (int i = length - 1; i >= 0; i--)
            {
                dest[pos + i] = (byte)(val & 0xFF);
                val >>= 8;
            }
            return pos + length;
        }
        private int WriteString(byte[] dest, int pos, string str, bool type)
        {
            if (string.IsNullOrEmpty(str))
                return 0;
            if (type)
                dest[pos++] = 0x2;
            byte[] bs = Encoding.ASCII.GetBytes(str);
            pos = PutInt(dest, pos, bs.Length, 2);
            bs.CopyTo(dest, pos);
            pos += bs.Length;
            return pos;
        }
        private int WriteString(byte[] dest, int pos, string str)
        {
            return WriteString(dest, pos, str, false);
        }
        private int WriteDouble(byte[] dest, int pos, double val)
        {
            dest[pos++] = 0;
            byte[] bd = BitConverter.GetBytes(val);
            for (int i = 0; i < 8; i++)
            {
                dest[pos++] = bd[7 - i];
            }
            return pos;
        }
        private int WriteByte(byte[] dest, int pos, byte b)
        {
            dest[pos++] = 0x1;
            dest[pos++] = b;
            return pos;
        }

        private void WriteHead(Stream dest, long datasize, double duration, double vcodec, double acodec,
            double framerate, double x, uint offset_b, int f1, int f2, bool reserve)
        {
            int framecount = f2 - f1 + 1;
            if (framecount <= 0)
                throw new Exception("帧不能为空！");

            double audiosize = 0;
            double videosize = 0;
            double audiocodec = acodec;
            double videocodec = vcodec;
            double lasttimestamp = 0;
            double lastkeyframetimestamp = 0;
            double lastkeyframelocation = 0;
            List<double> filepositions = new List<double>();
            List<double> times = new List<double>();

            long first_offset = 0;
            bool res = reserve;
            for (int i = f1; i <= f2; i++)
            {
                if ((first_offset == 0) && !(_parser.Tags[i] is FlvParser.ScriptTag))
                {
                    first_offset = _parser.Tags[i].Offset;
                }
                FlvParser.AudioTag atag = _parser.Tags[i] as FlvParser.AudioTag;
                if (atag != null)
                {
                    if (audiocodec < 0)
                        audiocodec = atag.CodecId;
                    audiosize += atag.DataSize + 11;
                    continue;
                }
                FlvParser.VideoTag vtag = _parser.Tags[i] as FlvParser.VideoTag;
                if (vtag != null)
                {
                    if (videocodec < 0)
                        videocodec = vtag.CodecId;
                    videosize += vtag.DataSize + 11;
                    lasttimestamp = Math.Round((vtag.TimeStamp - offset_b) * x) / 1000.0;
                    if (vtag.FrameType == "keyframe")
                    {
                        if (res)
                        {
                            lasttimestamp = vtag.TimeStamp / 1000.0;
                            res = false;
                        }
                        lastkeyframetimestamp = lasttimestamp;
                        lastkeyframelocation = vtag.Offset - first_offset;
                        filepositions.Add(lastkeyframelocation);
                        times.Add(lastkeyframetimestamp);
                    }
                    continue;
                }
            }
            FlvParser.ScriptTag meta = _parser.MetaTag;

            byte[] bhead = new byte[] {
                0x46, 0x4c, 0x56, // FLV
                0x01,             // Version 1
                0x05,             // 0000 0101, 有音频有视频
                0, 0, 0, 0x09,    // Header size, 9
                0, 0, 0, 0,       // Previous Tag Size #0
            };
            int pos = 0;
            byte[] buffer = new byte[63356];
            buffer[pos++] = 0x12; // script
            #region - 开始写 -
            for (int i = 0; i < 10; i++)
            {
                buffer[pos++] = 0;
            }
            pos = WriteString(buffer, pos, "onMetaData", true);
            buffer[pos++] = 0x08;
            pos = PutInt(buffer, pos, 26, 4);

            object o;
            double d;

            pos = WriteString(buffer, pos, "creator");
            pos = WriteString(buffer, pos, "bangbang93", true);

            pos = WriteString(buffer, pos, "metadatacreator");
            pos = WriteString(buffer, pos, "bangbang93", true);

            pos = WriteString(buffer, pos, "hasKeyframes");
            pos = WriteByte(buffer, pos, 1);
            pos = WriteString(buffer, pos, "hasVideo");
            pos = WriteByte(buffer, pos, 1);
            pos = WriteString(buffer, pos, "hasAudio");
            pos = WriteByte(buffer, pos, 1);
            pos = WriteString(buffer, pos, "hasMetadata");
            pos = WriteByte(buffer, pos, 1);
            pos = WriteString(buffer, pos, "canSeekToEnd");
            pos = WriteByte(buffer, pos, 0);

            pos = WriteString(buffer, pos, "duration");
            pos = WriteDouble(buffer, pos, duration);
            pos = WriteString(buffer, pos, "datasize");
            pos = WriteDouble(buffer, pos, datasize);
            pos = WriteString(buffer, pos, "videosize");
            pos = WriteDouble(buffer, pos, videosize);
            pos = WriteString(buffer, pos, "videocodecid");
            pos = WriteDouble(buffer, pos, videocodec);

            pos = WriteString(buffer, pos, "width");
            d = 512.0;
            if (meta.TryGet("width", out o))
                d = (double)o;
            pos = WriteDouble(buffer, pos, d);

            pos = WriteString(buffer, pos, "height");
            d = 384.0;
            if (meta.TryGet("height", out o))
                d = (double)o;
            pos = WriteDouble(buffer, pos, d);

            pos = WriteString(buffer, pos, "framerate");
            d = framerate > 0 ? framerate : (framecount / duration);
            pos = WriteDouble(buffer, pos, d);

            pos = WriteString(buffer, pos, "videodatarate");
            pos = WriteDouble(buffer, pos, videosize / 125.0 / duration);

            pos = WriteString(buffer, pos, "audiosize");
            pos = WriteDouble(buffer, pos, audiosize);
            pos = WriteString(buffer, pos, "audiocodecid");
            pos = WriteDouble(buffer, pos, audiocodec);
            pos = WriteString(buffer, pos, "audiosamplerate");
            d = 44100;
            if (meta.TryGet("audiosamplerate", out o))
                d = (double)o;
            pos = WriteDouble(buffer, pos, d);
            pos = WriteString(buffer, pos, "audiosamplesize");
            d = 16;
            if (meta.TryGet("audiosamplesize", out o))
                d = (double)o;
            pos = WriteDouble(buffer, pos, d);
            pos = WriteString(buffer, pos, "stereo");
            byte stereo = 1;
            if (meta.TryGet("stereo", out o))
                stereo = (byte)o;
            pos = WriteByte(buffer, pos, stereo);
            pos = WriteString(buffer, pos, "audiodatarate");
            pos = WriteDouble(buffer, pos, audiosize / 125.0 / duration);

            pos = WriteString(buffer, pos, "filesize");
            int filesize_pos = pos;
            pos += 9;

            pos = WriteString(buffer, pos, "lasttimestamp");
            pos = WriteDouble(buffer, pos, lasttimestamp);
            pos = WriteString(buffer, pos, "lastkeyframetimestamp");
            pos = WriteDouble(buffer, pos, lastkeyframetimestamp);
            pos = WriteString(buffer, pos, "lastkeyframelocation");
            pos = WriteDouble(buffer, pos, lastkeyframelocation);
            #endregion
            pos = WriteString(buffer, pos, "keyframes");
            buffer[pos++] = 3; // object
            pos = WriteString(buffer, pos, "filepositions");
            int file_positions = pos;
            pos = WriteArray(buffer, pos, filepositions);
            pos = WriteString(buffer, pos, "times");
            pos = WriteArray(buffer, pos, times);
            buffer[pos++] = 0;
            buffer[pos++] = 0;
            buffer[pos++] = 9; // 结束符

            // script tag 长度
            PutInt(buffer, 1, pos - 11, 3); // script 帧的 datasize
            pos = PutInt(buffer, pos, pos, 4);
            WriteDouble(buffer, filesize_pos, datasize + pos + bhead.Length); // filesize
            WriteArray(buffer, file_positions, filepositions, pos + bhead.Length + (reserve ? 27 : 0));

            dest.Write(bhead, 0, bhead.Length);
            dest.Write(buffer, 0, pos);
        }
        private int WriteArray(byte[] dest, int pos, List<double> ds)
        {
            return WriteArray(dest, pos, ds, 0.0);
        }
        private int WriteArray(byte[] dest, int pos, List<double> ds, double offset)
        {
            dest[pos++] = 0xa;
            pos = PutInt(dest, pos, ds.Count, 4);
            for (int i = 0; i < ds.Count; i++)
            {
                pos = WriteDouble(dest, pos, ds[i] + offset);
            }
            return pos;
        }

        private void PutTime(byte[] bs, int pos, uint value)
        {
            for (int i = 2; i >= 0; i--)
            {
                bs[pos + i] = (byte)(value & 0xff);
                value >>= 8;
            }
            bs[pos + 3] = (byte)(value & 0xff);
        }
        private byte[] GetH263Frame(uint timestamp, ushort width, ushort height)
        {
            long b = (1 << 16) | width;
            b = (b << 16) | height;
            b <<= 7;
            byte[] buffer = new byte[]{
                    0x09, 0, 0, 0x0c, // 视频帧 12 字节
                    0, 0, 0, 0,       // timestamp & ex
                    0, 0, 0,          // stream id
                    0x22, 0, 0, 0x84, 0, // InnerFrame, H.263
                    0, 0, 0, 0, 0, 0x12, 0x26, // 16~20:width x height
                    0, 0, 0, 0x17     // 此帧长度 23 字节
                };
            PutTime(buffer, 4, timestamp);
            for (int i = 0; i < 5; i++)
            {
                buffer[20 - i] = (byte)(b & 0xFF);
                b >>= 8;
            }
            return buffer;
        }
        #endregion

        private void inputFileBox_TextChanged(object sender, EventArgs e)
        {
            outputFileBox.Text = inputFileBox.Text.Substring(0, inputFileBox.Text.LastIndexOf(".")) + ".flv";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Filter="MP4|*.mp4";
            if (file.ShowDialog() == DialogResult.OK)
            {
                inputFileBox.Text = file.FileName;
            }
        }
    }
}
