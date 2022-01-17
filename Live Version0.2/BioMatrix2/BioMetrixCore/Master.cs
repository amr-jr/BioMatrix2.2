///
///    By: Amr Abdulnaser Bashnaini
///    Mail: amro.bashnaini335@gmail.com
///
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Threading;
using System.Net.NetworkInformation;
using System.IO;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace BioMetrixCore
{
    public partial class Master : Form
    {
        public CardView[] empCardViews;
        public OneCardView[] OneView;
        private int sizeOfEmpList = 8;
        private int EmpList = 1;
        static DeviceManipulator manipulator = new DeviceManipulator();
        public static ZkemClient objZkeeper;
        private bool isDeviceConnected = false;
        private bool isRunAsync = false;
        public static string serverAdress = "";
        public static string IP = "";
        public static int PORT = 0;
        public DataTable allEmpInfo;
        public static string empID;
        public static DateTime deviceTime;
        public static string AAM;
        public static string BBM;
        public string IDCol = "EMP_ID";
        public string TypeCol = "TYPE";
        public string NameCol = "ENAME";
        public string JobCol = "JOB_ID";
        public string TimeCol = "TRANS_DATE";
        public string DeptCol = "TEXT";
        public static String Con = "متصل بجهاز البصمة";
        public static String Dis = "غير متصل بجهاز البصمة";
        private string canNotConnectToDB_MSG = "غير متصل بقاعدة البيانات حاليا وسيتم اضافة البصمة تلقائيا للموظف : ";
        public const string acx_trans = "Transaction";
        Thread checkDeviceConnection = null;
        bool isRun;
        private bool isRunFirstOneTime;
        private bool isInitobjZkeeper;

        static bool isShowDBNotCon;

        public Master()
        {
            isShowDBNotCon = false;
            InitializeComponent();
            Label.CheckForIllegalCrossThreadCalls = false;
            checkDeviceConnection = new Thread(checkDeviceConnectionMethod);
        }

        private static OracleConnection TheConnection = null;

        public static OracleConnection getCon()
        {
            if (TheConnection == null)
            {
                TheConnection = new OracleConnection(UniversalStatic.GetConnectionString());
                TheConnection.Open();
            }


            if (TheConnection.State != System.Data.ConnectionState.Open)
            {

                TheConnection = new OracleConnection(UniversalStatic.GetConnectionString());
                TheConnection.Open();
            }

            if(TheConnection.State == ConnectionState.Open)
            {
                if (isShowDBNotCon)
                {
                    isShowDBNotCon = false;
                    manipulator.Check(objZkeeper, 1);
                }
            }
            LogClass.Write("DB Connection status is : " + TheConnection);
            return TheConnection;

        }


        public bool IsDeviceConnected
        {
            get { return isDeviceConnected; }
            set
            {
                isDeviceConnected = value;
                if (isDeviceConnected)
                {
                    panel4.Hide();
                }
            }
        }

        private void RaiseDeviceEvent(object sender, string actionType)
        {

            if (!ZkemClient.isAddFingerPrint)
            {
                isShowDBNotCon = true;
                DBConLabel.Text = canNotConnectToDB_MSG + empID;
                DBConLabel.Visible = false;
            }
            else
            {
                DBConLabel.Visible = false;

                if (actionType.Equals(acx_trans))
                {
                    DataRow dr = prepareEmpInfoDataRow();
                    allEmpInfo.Rows.Add(dr);
                    addEmpCardView(dr);
                    addOneCardView(dr);
                }
            }
        }


        private void btnConnect_Click(object sender, EventArgs e)
        {
            Connect_Click();
        }

        private void Connect_Click()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                if (IsDeviceConnected)
                {
                    IsDeviceConnected = false;
                    this.Cursor = Cursors.Default;

                    return;
                }


                string ipAddress = Properties.Settings.Default.IpAddress;

                if (ipAddress.Equals("0"))
                {
                    ipAddress = tbxDeviceIP.Text.Trim();

                    Properties.Settings.Default.IpAddress = tbxDeviceIP.Text.Trim();
                    Properties.Settings.Default.Save();
                }

                string port = tbxPort.Text.Trim();
                string server = textBox1.Text.Trim();


                int portNumber = 4370;
                LogClass.Write("1 Try to connect to the device with IP " + ipAddress);
                objZkeeper = new ZkemClient(RaiseDeviceEvent);
                IsDeviceConnected = objZkeeper.Connect_Net(ipAddress, portNumber);
                LogClass.Write("1 Try to connect to Set the Device Time");
                manipulator.SetDeviceTime(objZkeeper, int.Parse(tbxMachineNumber.Text.Trim()));
                LogClass.Write("1 Go to check function");
                manipulator.Check(objZkeeper, int.Parse(tbxMachineNumber.Text.Trim()));

                isInitobjZkeeper = true;
                serverAdress = server;
                IP = ipAddress;
                PORT = portNumber;

                if (IsDeviceConnected)
                {
                    isRunAsync = false;

                    isRunFirstOneTime = false;
                    isRun = true;
                    checkDeviceConnection.Start();
                }

            }
            catch (Exception x7)
            {
                LogClass.Write("Exception 7 : " + x7.Message);
            }
            this.Cursor = Cursors.Default;

        }

        public void checkDeviceConnectionMethod()
        {
            while (isRun)
            {
                try
                {
                    if (IP != null)
                    {
                        PingReply reply = new Ping().Send(IP);

                        IPStatus SuccessState = IPStatus.Success;

                        if (reply.Status == SuccessState)
                        {
                            label4.Text = Con;
                            label4.BackColor = Color.LightGreen;

                            Thread.Sleep(5000);

                            if (!isRunFirstOneTime)
                            {
                                if (this.InvokeRequired)
                                {
                                    this.BeginInvoke((MethodInvoker)delegate ()
                                    {

                                        if (!isInitobjZkeeper)
                                        {
                                            objZkeeper = new ZkemClient(RaiseDeviceEvent);
                                            LogClass.Write("2 connecting...");

                                            IsDeviceConnected = objZkeeper.Connect_Net(IP, PORT);
                                            LogClass.Write("2 Setting Device Time...");

                                            manipulator.SetDeviceTime(objZkeeper, int.Parse(tbxMachineNumber.Text.Trim()));
                                            LogClass.Write("2 Checking the log...");

                                            manipulator.Check(objZkeeper, int.Parse(tbxMachineNumber.Text.Trim()));
                                        }
                                        LogClass.Write("4 Setting Device Time...");

                                        manipulator.SetDeviceTime(objZkeeper, int.Parse(tbxMachineNumber.Text.Trim()));
                                        LogClass.Write("4 do the interface...");

                                        allEmpInfo = setAllEmpInfo();
                                        empCardViews = new CardView[sizeOfEmpList];
                                        OneView = new OneCardView[EmpList];
                                        int endIndex = allEmpInfo.Rows.Count - 1;
                                        for (int i = endIndex; i >= 0; i--)
                                        {
                                            addEmpCardView(allEmpInfo.Rows[i]);
                                        }
                                        addOneCardView(allEmpInfo.Rows[0]);
                                    });
                                }
                                else
                                {
                                    if (!isInitobjZkeeper)
                                    {
                                        objZkeeper = new ZkemClient(RaiseDeviceEvent);
                                        LogClass.Write("connecting...");
                                        IsDeviceConnected = objZkeeper.Connect_Net(IP, PORT);
                                        LogClass.Write("Setting Device Time...");
                                        manipulator.SetDeviceTime(objZkeeper, int.Parse(tbxMachineNumber.Text.Trim()));
                                        LogClass.Write("Checking the log...");
                                        manipulator.Check(objZkeeper, int.Parse(tbxMachineNumber.Text.Trim()));
                                        manipulator.SetDeviceTime(objZkeeper, int.Parse(tbxMachineNumber.Text.Trim()));
                                    }
                                    LogClass.Write("5 Setting Device Time...");
                                    manipulator.SetDeviceTime(objZkeeper, int.Parse(tbxMachineNumber.Text.Trim()));
                                    LogClass.Write("5 do the interface...");
                                    allEmpInfo = setAllEmpInfo();
                                    empCardViews = new CardView[sizeOfEmpList];
                                    OneView = new OneCardView[EmpList];

                                    buildEmpCardViews();

                                    addOneCardView(allEmpInfo.Rows[0]);
                                }

                                isRunFirstOneTime = true;
                            }
                        }
                        else
                        {
                            LogClass.Write(" Ip Status is " + reply.Status);
                            label4.Text = Dis;
                            label4.BackColor = Color.Red;
                            LogClass.Write("Witing For Device On");
                            Thread.Sleep(40000);

                            try
                            {
                                if (objZkeeper != null)
                                {
                                    objZkeeper.Disconnect();
                                    objZkeeper = null;
                                }
                            }
                            catch (Exception X97)
                            {
                                LogClass.Write("Exception 97 : " + X97.Message);
                            }

                            isRunFirstOneTime = false;
                            isInitobjZkeeper = false;

                        }

                    }
                }
                catch (Exception X89)
                {
                    isRunFirstOneTime = false;
                    isInitobjZkeeper = false;
                    LogClass.Write("Exception 89 : " + X89.Message);
                }
            }
        }

        private DataRow prepareEmpInfoDataRow()
        {

            DataRow dr = allEmpInfo.NewRow();

            dr[IDCol] = empID;
            dr[TimeCol] = deviceTime.ToString("hh") + ":" + deviceTime.ToString("mm");
            dr[TypeCol] = BBM;
            DataTable empInfo = UniversalStatic.selectQuery("SELECT DEPT_ID  , EMP_ID , ENAME , JOB_ID  ,(SELECT TEXT FROM HR_DEPARTMENT D WHERE D.DEPT_ID = E.DEPT_ID AND COMP_ID = 1)TEXT FROM HR.HR_EMPLOYEE E WHERE E.EMP_ID = '" + empID + "' AND COMP_ID='1'");
            if (empInfo.Rows.Count > 0)
            {
                dr[NameCol] = empInfo.Rows[0][NameCol].ToString();
                dr[JobCol] = empInfo.Rows[0][JobCol].ToString();
                dr[DeptCol] = empInfo.Rows[0][DeptCol].ToString();
            }
            else
            {
                dr[NameCol] = "اسم الموظف ليس مضاف";
                dr[JobCol] = "لاتوجد بيانات";
                dr[DeptCol] = "لاتوجد بيانات";
            }

            return dr;
        }

        private void addEmpCardView(DataRow dr)
        {
            string imgPath = @"\\172.16.24.19\imagefinger\" + dr[IDCol].ToString() + ".jpg";

            Image empIMG;

            if (!File.Exists(imgPath))
            {
                empIMG = Properties.Resources.user;
            }
            else
            {
                empIMG = Image.FromFile(imgPath);
            }

            CardView empCardView = new CardView();

            empCardView.empIMG = empIMG;
            empCardView.empID = dr[IDCol].ToString();
            empCardView.empName = dr[NameCol].ToString();
            empCardView.empbb = dr[TypeCol].ToString();

            DateTime dt = DateTime.Parse(dr[TimeCol].ToString());
            string empTime = dt.ToString("hh:mm");

            empCardView.empTime = empTime;

            flowLayoutPanel1.Controls.Add(empCardView);
            flowLayoutPanel1.Controls.SetChildIndex(empCardView, 0);

            if (flowLayoutPanel1.Controls.Count == 8)
            {
                flowLayoutPanel1.Controls.RemoveAt(7);
            }

        }

        private void addOneCardView(DataRow dr)
        {
            string imgPath = @"\\172.16.24.19\imagefinger\" + dr[IDCol].ToString() + ".jpg";

            Image empIMG;

            if (!File.Exists(imgPath))
            {
                empIMG = Properties.Resources.user;
            }
            else
            {
                empIMG = Image.FromFile(imgPath);
            }

            DateTime dt = DateTime.Parse(dr[TimeCol].ToString());
            string empTime = dt.ToString("hh:mm");
            string empDate = dt.ToString("yyyy" + "/" + "MM" + "/" + "dd");

            OneCardView one = new OneCardView();
            one.empIMG = empIMG;
            one.empID = dr[IDCol].ToString();
            one.empName = dr[NameCol].ToString();
            one.empTime = empTime;
            one.empJob = dr[JobCol].ToString();
            one.empDept = dr[DeptCol].ToString();
            one.empaa = dr[TypeCol].ToString();
            one.empbb = AAM;
            one.empDate = empDate;

            OneViewPanel.Controls.Add(one);
            OneViewPanel.Controls.SetChildIndex(one, 0);

            if (OneViewPanel.Controls.Count == 2)
            {
                OneViewPanel.Controls.RemoveAt(1);
            }
        }

        private void buildEmpCardViews()
        {
            for (int i = 0; i < allEmpInfo.Rows.Count; i++)
            {
                string imgPath = @"\\172.16.24.19\imagefinger\" + allEmpInfo.Rows[i][IDCol].ToString() + ".jpg";

                Console.WriteLine("imgPath : " + imgPath);

                Image empIMG;

                if (!File.Exists(imgPath))
                {
                    empIMG = Properties.Resources.user;
                }
                else
                {
                    empIMG = Image.FromFile(imgPath);
                }

                empCardViews[i] = new CardView();

                empCardViews[i].empIMG = empIMG;
                empCardViews[i].empID = allEmpInfo.Rows[i][IDCol].ToString();
                empCardViews[i].empName = allEmpInfo.Rows[i][NameCol].ToString();
                empCardViews[i].empbb = allEmpInfo.Rows[i][TypeCol].ToString();

                DateTime time;
                if (DateTime.TryParse(allEmpInfo.Rows[i][TimeCol].ToString(), out time))
                {
                    empCardViews[i].empTime = time.ToString("hh:mm");
                }
                else
                {
                    empCardViews[i].empTime = allEmpInfo.Rows[i][TimeCol].ToString();
                }

                flowLayoutPanel1.Controls.Add(empCardViews[i]);
            }

        }
        private DataTable setAllEmpInfo()
        {
            DataTable empsInfo = UniversalStatic.selectQuery("SELECT ATT.EMP_ID, E.ENAME, E.JOB_ID, E.DEPT_ID, ATT.TYPE, ATT.TRANS_DATE,(SELECT TEXT FROM HR_DEPARTMENT D WHERE D.DEPT_ID = E.DEPT_ID AND COMP_ID =1)TEXT FROM HR.HR_ATTENDANCE ATT LEFT JOIN  HR.HR_EMPLOYEE E ON(ATT.EMP_ID = E.EMP_ID) WHERE TO_CHAR(TRANS_DATE, 'YYYY') = TO_CHAR(SYSDATE, 'YYYY') AND DEVICE_IP = '" + IP + "' ORDER BY  ATT.TRANS_DATE DESC FETCH FIRST 8 ROWS ONLY");

            return empsInfo;
        }

        private void tbxPort_TextChanged(object sender, EventArgs e)
        { UniversalStatic.ValidateInteger(tbxPort); }

        private void tbxMachineNumber_TextChanged(object sender, EventArgs e)
        { UniversalStatic.ValidateInteger(tbxMachineNumber); }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label3.Text = DateTime.Now.ToString("hh" + ": " + "mm" + ": " + "ss");
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }
        private void Master_Shown(Object sender, EventArgs e)
        {
            string ip = Properties.Settings.Default.IpAddress;

            isRunAsync = true;
            MyMethodAsync();
            if (!ip.Equals("0"))
            {
                panel4.Hide();
            }
            timer1.Start();

        }
        private void Master_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.E)
            {
                MessageBox.Show("يرجى إعادة تشغيل النظـام");
                Properties.Settings.Default.IpAddress = "0";
                Properties.Settings.Default.Save();
                isRunAsync = false;
            }

        }

        Task<bool> longRunningTask = null;

        public async Task MyMethodAsync()
        {
            if (longRunningTask == null || longRunningTask.IsCompleted || longRunningTask.Result)
            {
                longRunningTask = LongRunningOperationAsync();
                // independent work which doesn't need the result of LongRunningOperationAsync can be done here
                bool dd = longRunningTask.IsCompleted;
                //and now we call await on the task 
                bool result = await longRunningTask;
                //use the result 
                Console.WriteLine(result);
            }
        }

        public async Task<bool> LongRunningOperationAsync() // assume we return an int from this long running operation 
        {
            int counter = 0;
            string ip = Properties.Settings.Default.IpAddress;

            if (!ip.Equals("0"))
            {
                while (!IsDeviceConnected)
                // while (isRunAsync)
                {
                    string ipAddress = Properties.Settings.Default.IpAddress;

                    Console.WriteLine("Hello");

                    if (!ipAddress.Equals("0"))
                    {
                        Connect_Click();
                        if (!IsDeviceConnected)
                        {
                            await Task.Delay(4000); // 1 second delay
                            counter++;
                        }
                    }

                }
            }
            return true;
        }

    }
}
