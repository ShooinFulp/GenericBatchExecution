using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GenericBatchExecution
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private const string GET_DATABASES_NAME = "SELECT NAME FROM MASTER.DBO.SYSDATABASES ORDER BY NAME";

        //所有数据库名
        private DataTable dt = null;

        private void btnTest_Click(object sender, EventArgs e)
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection();
                conn.ConnectionString = string.Format("server={0};user={1};pwd={2}", txtIp.Text.Trim(), txtUser.Text.Trim(), txtPwd.Text.Trim());
                conn.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "连接失败！");
                return;
            }
            SqlDataAdapter adp = new SqlDataAdapter(GET_DATABASES_NAME, conn);
            dt = new DataTable();
            adp.Fill(dt);
            MessageBox.Show("数据库数量：" + dt.Rows.Count + "其中第一个：" + dt.Rows[0][0].ToString(), "连接成功");
            conn.Close();
            conn.Dispose();
        }

        private void btnExec_Click(object sender, EventArgs e)
        {
            //清理日志
            txtLog.Clear();
            Thread thread = new Thread(new ThreadStart(
                () =>
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        string database = item[0].ToString();
                        string connStr = string.Format("server={0};user={1};pwd={2};database={3};", txtIp.Text.Trim(), txtUser.Text.Trim(), txtPwd.Text.Trim(), database);
                        try
                        {
                            WriteLog("current：" + connStr);
                            SqlConnection conn = new SqlConnection(connStr);
                            conn.Open();
                            SqlCommand cmd = new SqlCommand(txtSql.Text, conn);
                            int result = cmd.ExecuteNonQuery();
                            WriteLog("rows：" + result);
                        }
                        catch (Exception ex)
                        {
                            WriteLog("error：" + ex.Message);
                        }
                    }
                }
                ));
            thread.IsBackground = false;
            thread.Start();
        }

        private void WriteLog(string msg)
        {
            msg = DateTime.Now.ToString() + "   " + msg + "\r\n";
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => { txtLog.AppendText(msg); txtLog.ScrollToCaret(); }));
            }
            else
            {
                txtLog.AppendText(msg);
                txtLog.ScrollToCaret();
            }
        }
    }
}
