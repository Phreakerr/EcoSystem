using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EcoSystem;
using System.Threading;

namespace EcoSystem_Launcher
{
    public partial class Launcher : Form
    {
        public Launcher()
        {
            InitializeComponent();
            btnLaunch.Enabled = false;
        }

        private void btnLaunch_Click(object sender, EventArgs e)
        {
            Visible = false;

            Thread thread = new Thread(() =>
            {
                EcoSystemGame game = new EcoSystemGame();
                game.Run();
            });

            thread.Start();
            thread.Join();

            Application.Exit();
        }
    }
}
