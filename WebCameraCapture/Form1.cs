using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using WindowsFormsApp1.Properties;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        Mat frame;
        VideoCapture capture;
        Bitmap bmp;
        Graphics graphic;
        bool flgClick = false;
        private System.Media.SoundPlayer player = null;

        // WEBカメラからの入力サイズ
        int WIDTH = Properties.Settings.Default.cameraWidth;
        int HEIGHT = Properties.Settings.Default.cameraHeight;

        // 検知対象のキーコード
        Keys keyCode = (Keys)Properties.Settings.Default.keyCode;

        // キャプチャ時の効果音
        string SoundFile = Properties.Settings.Default.soundFile;

        // キャプチャ画像保存フォルダ
        string SaveImageFolder = Properties.Settings.Default.SaveImageFolder;

        // キー入力チェック用
        [DllImport("user32")]
        static extern short GetAsyncKeyState(Keys vKey);

        public Form1()
        {
            InitializeComponent();

            //カメラ画像取得用のVideoCapture作成
            capture = new VideoCapture(0);
            if (!capture.IsOpened())
            {
                MessageBox.Show("カメラが見つかりませんでした");
                this.Close();
            }

            capture.FrameWidth = WIDTH;
            capture.FrameHeight = HEIGHT;

            //取得先のMat作成
            frame = new Mat(HEIGHT, WIDTH, MatType.CV_8UC3);

            //表示用のBitmap作成
            bmp = new Bitmap(frame.Cols, frame.Rows, (int)frame.Step(), System.Drawing.Imaging.PixelFormat.Format24bppRgb, frame.Data);


            //フォームの横幅・高さを出力サイズ+αに設定
            this.Width = frame.Cols + 40;
            this.Height = frame.Rows + 95;

            //PictureBoxを出力サイズに合わせる
            pictureBox1.Width = frame.Cols;
            pictureBox1.Height = frame.Rows;

            //描画用のGraphics作成
            graphic = pictureBox1.CreateGraphics();

            //画像取得スレッド開始
            backgroundWorker1.RunWorkerAsync();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // キーボード入力検知用タイマー
            timer1.Start();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {

            if (GetAsyncKeyState(keyCode) < 0)
            {
                // 指定したキーコードの入力を検知した場合
                if (flgClick == false)
                {
                    // ボタンクリック
                    button1.PerformClick();
                    flgClick = true;
                }
            }
            else
            {
                flgClick = false;
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            String fileName = now.ToString("yyyyMMdd_HHmmss_ff");

            frame.SaveImage(SaveImageFolder + fileName + ".jpg");

            // 効果音再生
            player = new System.Media.SoundPlayer(SoundFile);
            player.Play();
    }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //スレッドの終了を待機
            backgroundWorker1.CancelAsync();
            while (backgroundWorker1.IsBusy)
                Application.DoEvents();
        }

        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = (BackgroundWorker)sender;

            while (!backgroundWorker1.CancellationPending)
            {
                //画像取得
                capture.Grab();
                NativeMethods.videoio_VideoCapture_operatorRightShift_Mat(capture.CvPtr, frame.CvPtr);

                bw.ReportProgress(0);
            }
        }

        private void BackgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //描画
            graphic.DrawImage(bmp, 0, 0, frame.Cols, frame.Rows);
        }

    }
}
