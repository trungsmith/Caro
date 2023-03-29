using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Caro
{
    public partial class Form1 : Form
    {
        //Kỹ thuật tách code
        #region Properties
        ChessBoardManager ChessBoard;
        SocketManager socket;
        #endregion
        //
        public  Form1()
        {
            InitializeComponent();

            Control.CheckForIllegalCrossThreadCalls = false;
            
            ChessBoard = new ChessBoardManager(pnlChessBoard, txbPlayername, pcbMark);
            ChessBoard.EndedGame += ChessBoard_EndedGame;
            ChessBoard.PlayerMarked += ChessBoard_PlayerMarked;

            prcbCooldown.Step = Cons.COOL_DOWN_STEP;
            prcbCooldown.Maximum = Cons.COOL_DOWN_TIME;
            prcbCooldown.Value = 0;

            tmCooldown.Interval = Cons.COOL_DOWN_INTERVAL;

            socket = new SocketManager();
            NewGame();


            prcbCooldown.Value = 0;
            tmCooldown.Stop();
            
        }

        #region Methods
        void EndGame()
        {
            tmCooldown.Stop();
            pnlChessBoard.Enabled = false;
            undoToolStripMenuItem.Enabled = false;
            //MessageBox.Show("Kết thúc!");
        }

        void NewGame()
        {
            prcbCooldown.Value = 0;
            tmCooldown.Stop();
            undoToolStripMenuItem.Enabled = true;
            ChessBoard.DrawChessBoard();
  
        }
        void Quit()
        {
            Application.Exit();
        }
        void Undo()
        {
            ChessBoard.Undo();
            prcbCooldown.Value = 0;
            
        }
        
        private void ChessBoard_PlayerMarked(object sender, ButtonClickEvent e)
        {
            tmCooldown.Start();
            pnlChessBoard.Enabled = false;
            prcbCooldown.Value = 0;

            socket.Send(new SocketData((int)SocketCommand.SEND_POINT," ",e.ClickedPoint));

            undoToolStripMenuItem.Enabled = false;
            Listen();
        }

        private void ChessBoard_EndedGame (object sender, EventArgs e)
        {
            //EndGame();
            socket.Send(new SocketData((int)SocketCommand.END_GAME, " ", new Point()));
        }
        private void tmCooldown_Tick(object sender, EventArgs e)
        {
            prcbCooldown.PerformStep();

            if(prcbCooldown.Value >= prcbCooldown.Maximum)
            {
                
                EndGame();
                socket.Send(new SocketData((int)SocketCommand.TIME_OUT, " ", new Point()));
            }
        }

        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewGame();
            socket.Send(new SocketData((int)SocketCommand.NEW_GAME, " ", new Point()));
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Undo();
            socket.Send(new SocketData((int)SocketCommand.UNDO, "", new Point()));
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Quit();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn thoát?", "Thông báo", MessageBoxButtons.OKCancel) != System.Windows.Forms.DialogResult.OK)
            { 
                e.Cancel = true;
            }
            else
            {
                try
                {
                    socket.Send(new SocketData((int)SocketCommand.QUIT, " ", new Point()));
                }
                catch
                {

                }
            }
        }
        private void btnLAN_Click(object sender, EventArgs e)
        {
            socket.IP = txtIP.Text;

            if (!socket.ConnectServer())
            {
                socket.isServer= true;
                pnlChessBoard.Enabled= true;
                socket.CreateServer();
            }
            else
            {
                socket.isServer= false;
                pnlChessBoard.Enabled= false;   
                Listen();
      
            }
           
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            txtIP.Text = socket.GetLocalIPv4(NetworkInterfaceType.Wireless80211);

            if (string.IsNullOrEmpty(txtIP.Text))
            {
                txtIP.Text = socket.GetLocalIPv4(NetworkInterfaceType.Ethernet);
            }
        }

        void Listen()
        {

            Thread listenThread = new Thread(() =>
            {
                try
                {
                    SocketData data = (SocketData)socket.Receive();
                    ProcessData(data);
                }
                catch
                {

                }
            });
            listenThread.IsBackground = true;
            listenThread.Start();
        }
            
        

        private void ProcessData(SocketData data)
        {
            switch (data.Command)
            {
                case (int)SocketCommand.NOTIFY:
                    MessageBox.Show(data.Message);
                    break;
                case (int)SocketCommand.NEW_GAME:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        NewGame();
                    }));
                    break;
                case (int)SocketCommand.SEND_POINT:
                    this.Invoke((MethodInvoker)(() => 
                    {
                        prcbCooldown.Value = 0;
                        pnlChessBoard.Enabled = true;
                        tmCooldown.Start();
                        ChessBoard.OtherPlayerMarked(data.Point);
                        undoToolStripMenuItem.Enabled = true;
                    }));
                    break;
                case (int)SocketCommand.UNDO:
                    Undo();
                    prcbCooldown.Value = 0;
                    break;
                case (int)SocketCommand.QUIT:
                    tmCooldown.Stop();
                    MessageBox.Show("Đối thủ đã thoát!","Thông báo");
                    break;
                case (int)SocketCommand.END_GAME:
                    MessageBox.Show("Đã có 5 con trên 1 hàng! Kết thúc trò chơi!", "Thông báo");
                    break;
                case (int)SocketCommand.TIME_OUT:
                    MessageBox.Show("Hết giờ!", "Thông báo");
                    break;
                default: 
                    break;
            }
            Listen();
        }

        #endregion
    }
}
