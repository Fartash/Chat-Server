using System;
using System.Collections;
using System.Text;
using System.Data.SqlClient;
using System.Net;
using System.Data.SqlTypes;
using System.Data;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using MySql.Data.MySqlClient;


namespace ChatServer
{
    public delegate void disconnectionHandler(object sender, disconnectionEventArgs r);
    public delegate void clientConnectedHandler(object a, clientConnectionEventArgs b);
    /// <summary>
    /// Summary description for chatServer.
    /// </summary>
    public class chatServer
    {
        private int m_port;
        private string[] IDs;
        private MySqlCommand insertNewUser;
        private string connString;
        private MySqlCommand fetchIDpass;
        private MySqlCommand fetchID;
        private SqlConnection connection;
        private string userName;
        private string passWord;
        private Thread clientThread;
        private ArrayList clientThreads;
        private Hashtable socketArray;
        private Socket sock1;
        private Socket sock2;
        private Thread connThread;
        private IPEndPoint ipep;
        private MySqlConnection myConnection;
        public chatServer()
        {

            string server = "localhost";
            string database = "chatserver";
            string uid = "root";
            string password = "sesame";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            myConnection = new MySqlConnection(connectionString);
            fetchIDpass = new MySqlCommand("fetchpass", myConnection);
            fetchIDpass.CommandType = CommandType.StoredProcedure;


            fetchIDpass.Parameters.Add("@identifier", MySql.Data.MySqlClient.MySqlDbType.VarChar,
                10, "id");


            //sqlCommand fetching all registered ID's

            fetchID = new MySqlCommand("fetchid", new MySqlConnection(connectionString));
            fetchID.CommandType = CommandType.StoredProcedure;
            insertNewUser = new MySqlCommand("insertnewchatter", new MySqlConnection(connectionString));
            insertNewUser.CommandType = CommandType.StoredProcedure;
            insertNewUser.Parameters.Add("@identifier", MySql.Data.MySqlClient.MySqlDbType.VarChar,
                10, "id");
            insertNewUser.Parameters.Add("@password", MySql.Data.MySqlClient.MySqlDbType.VarChar,
                10, "pass");

            IDs = new string[100];

            clientThreads = new ArrayList();
            socketArray = new Hashtable();
            connThread = new Thread(new ThreadStart(connectClients));
            connThread.Start();
        }
        public event clientConnectedHandler clientConnected;
        public void connectClients()
        {
            int a = 1;
            byte[] IDpassword;
            byte[] idB;
            byte[] passB;
            string id;
            string pass;
            bool check = false;
            ipep = new IPEndPoint(IPAddress.Any, m_port);
            sock1 = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            try
            {
                sock1.Bind(ipep);
            }
            catch (SocketException op)
            {
                MessageBox.Show(op.Message);
            }
            for (; ; )
            {
                IDpassword = new byte[20];
                idB = new byte[10];
                passB = new byte[10];
                sock1.Listen(10);         //The argument is the maximum length of the pending connections queue
                sock2 = sock1.Accept();
                a = sock2.Receive(IDpassword);
                if (a == 0)
                {
                    sock2.Shutdown(SocketShutdown.Both);
                    sock2.Close();
                    continue;
                }
                check = checkIDpassword(IDpassword);
                if (check == false)
                {
                    byte[] y = new byte[14];
                    Encoding.ASCII.GetBytes("wrong password", 0, 14, y, 0);
                    sock2.Send(y);
                    sock2.Close();
                    continue;
                }
                else
                {
                    int i = 0, n = 0, eachItem = 0;
                    bool flag = false;
                    string temp;
                    byte[] x = new byte[1100];
                    foreach (string item in IDs)
                        if (item != null)
                        {
                            temp = item.TrimEnd();
                            flag = true;
                            n = eachItem;
                            Encoding.ASCII.GetBytes(temp, 0, temp.Length, x, n);
                            n += temp.Length;


                            i = 10 - (temp.Length);
                            for (int w = 0; w < i; ++w)
                            {
                                Encoding.ASCII.GetBytes("\0", 0, 1, x, n);
                                ++n;
                            }

                            if (isOnline(temp))
                            {
                                Encoding.ASCII.GetBytes("$", 0, 1, x, n);
                            }
                            else
                            {
                                Encoding.ASCII.GetBytes("%", 0, 1, x, n);
                            }

                            eachItem += 11;
                        }
                        else
                        {
                            if (flag == false)
                                Encoding.ASCII.GetBytes("welcome", 0, 7, x, 0);
                        }
                    int p = sock2.Send(x);
                    if (p == 0)
                        MessageBox.Show("Agreement was not Sent!");

                    for (int b = 0; b < 10; ++b)
                        idB[b] = IDpassword[b];
                    clientConnectionEventArgs ea = new clientConnectionEventArgs(idB);
                    clientConnected(this, ea);


                    //vaghti client be server vasl mishavad yek baste
                    //be toole 20 byte havie id va pass miferestad
                    //ke dar code zir be 2 ghesmate id va pass
                    //joda misgavad


                    //Fires an event to notify the User of the class
                    //that a client is connected to the server

                    //generating the ID and password
                    //from the received BYTE array

                    id = Encoding.ASCII.GetString(idB, 0, idB.Length);

                    try
                    {
                        socketArray.Add(id, sock2);
                    }
                    catch (ArgumentException m)
                    {
                        //eventlog  , error
                    }
                    //notifying all the users , the Connection of the new user
                    notify();

                    receivingThread rt = new receivingThread();
                    rt.sock = sock2;
                    rt.Disconnection += new disconnectionHandler(rt_Disconnection);
                    rt.messageReceived += new messageReceptionHandler(sendMessage);
                    clientThread = new Thread(new ThreadStart(rt.receiving));
                    clientThreads.Add(clientThread);
                    clientThread.Start();
                }
            }
        }
        private void notify()
        {
            int i = 0, n = 0, eachItem = 10;
            Socket temp;
            string temp1;
            byte[] x = new byte[1024];
            //alamati ke be tarafe moghabel neshan midahad
            //ke in message havie ID ha ast
            Encoding.ASCII.GetBytes("!!!!!!!!!!", 0, 10, x, 0);
            this.fetchIDs();
            foreach (string item in IDs)
                if (item != null)
                {
                    temp1 = item.TrimEnd();
                    n = eachItem;
                    Encoding.ASCII.GetBytes(temp1, 0, temp1.Length, x, n);
                    n += temp1.Length;


                    i = 10 - (temp1.Length);
                    for (int w = 0; w < i; ++w)
                    {
                        Encoding.ASCII.GetBytes("\0", 0, 1, x, n);
                        ++n;
                    }

                    if (isOnline(temp1))
                    {
                        Encoding.ASCII.GetBytes("$", 0, 1, x, n);
                    }
                    else
                    {
                        Encoding.ASCII.GetBytes("%", 0, 1, x, n);
                    }

                    eachItem += 11;
                }
            foreach (DictionaryEntry item in socketArray)
            {
                temp = (Socket)item.Value;
                temp.Send(x);
            }
        }
        private void rt_Disconnection(object sender, disconnectionEventArgs e)
        {
            int i = 0, n = 0, eachItem = 10;
            Socket temp;
            string temp1;
            byte[] x = new byte[1024];
            //alamati ke be tarafe moghabel neshan midahad
            //ke in message havie ID ha ast
            Encoding.ASCII.GetBytes("!!!!!!!!!!", 0, 10, x, 0);
            this.fetchIDs();
            foreach (string item in IDs)
                if (item != null)
                {
                    temp1 = item.TrimEnd();
                    n = eachItem;
                    Encoding.ASCII.GetBytes(temp1, 0, temp1.Length, x, n);
                    n += temp1.Length;


                    i = 10 - (temp1.Length);
                    for (int w = 0; w < i; ++w)
                    {
                        Encoding.ASCII.GetBytes("\0", 0, 1, x, n);
                        ++n;
                    }

                    if (isOnline(temp1))
                    {
                        Encoding.ASCII.GetBytes("$", 0, 1, x, n);
                    }
                    else
                    {
                        Encoding.ASCII.GetBytes("%", 0, 1, x, n);
                    }

                    eachItem += 11;
                }
            foreach (DictionaryEntry item in socketArray)
            {
                temp = (Socket)item.Value;
                temp.Send(x);
            }
        }
        private bool isOnline(string id)
        {
            string temp, temp1;
            int position = 0;
            foreach (DictionaryEntry item in socketArray)
            {
                temp = (string)item.Key;
                position = temp.IndexOf('\0');
                if (position != -1)
                    temp1 = temp.Substring(0, position);
                else
                    temp1 = temp;
                if (temp1 == id)
                    return true;
            }
            return false;
        }
        private bool checkIDpassword(byte[] idPass)
        {
            string temp = "";
            int z = 0, x = 0, c = 0;
            byte[] idB = new byte[10];
            byte[] passB = new byte[10];
            string id;
            string pass;
            for (int i = 0; i < 10; ++i)
                idB[i] = idPass[i];
            for (int j = 10, k = 0; j < 20; ++j, ++k)
                passB[k] = idPass[j];

            while (z < 10)
                if (idB[z] != 0)
                    ++z;
                else
                    break;
            while (x < 10)
                if (passB[x] != 0)
                    ++x;
                else
                    break;

            id = Encoding.ASCII.GetString(idB, 0, z);
            pass = Encoding.ASCII.GetString(passB, 0, x);



            fetchIDpass.Parameters["@identifier"].Value = id;
            try
            {
                fetchIDpass.Connection.Open();
                temp = (string)fetchIDpass.ExecuteScalar();
                fetchIDpass.Connection.Close();
            }
            catch (MySqlException u)
            {
                MessageBox.Show(u.Message);
            }
            //trimEnd white space haye dakhele string ra
            //hazf mikonad , in baraye string hayee ke az
            //database bargasht dade mishavad kheili mohem ast
            //chon be tore mesal agar toole field dar DB 10 bashad
            //va reshteye ma 6 bashad , white space ba code 32
            //dar entehaye an mimanad ke bayad hazf shavad
            string temp1 = temp.TrimEnd(null);

            if (temp1 == pass)
                return true;
            else
                return false;
        }

        public string[] fetchIDs()
        {
            int i = 0;
            MySqlDataReader theReader;

            try
            {
                //myConnection.Open();
                fetchID.Connection.Open();
                theReader = fetchID.ExecuteReader();
                while (theReader.Read())
                {
                    IDs[i] = theReader.GetString(0);
                    IDs[i] = IDs[i].TrimEnd();
                    ++i;
                }

            }
            catch (MySqlException l)
            {
                MessageBox.Show(l.Message);
            }
            finally
            {
                //myConnection.Close();
                fetchID.Connection.Close();
            }
            return IDs;
        }
        private void sendMessage(object sender, messageReceptionEventArgs eveArgs)
        {
            string t = null;
            if (eveArgs.Disconnection)
            {
                foreach (DictionaryEntry item in socketArray)
                {
                    if (eveArgs.theSocket == item.Value)
                    {
                        t = (string)item.Key;
                    }
                }
                if (t != null)
                {
                    socketArray.Remove(t);
                    clientConnectionEventArgs sd = new clientConnectionEventArgs(t, true);
                    clientConnected(this, sd);
                }
            }
            else
            {
                Socket s;
                byte[] id = new byte[10];
                byte[] msg = new byte[1014];
                for (int i = 0; i < 10; ++i)
                    if (eveArgs.message[i] != 0)
                        id[i] = eveArgs.message[i];
                for (int j = 10, k = 0; j < 1014; ++j, ++k)
                    if (eveArgs.message[j] != 0)
                        msg[k] = eveArgs.message[j];

                s = (Socket)socketArray[Encoding.ASCII.GetString(id, 0, id.Length)];
                if (s != null) //if the destination user is online
                    s.Send(eveArgs.message);
            }
        }
        public string[] insertNewChatter(string id, string pass)
        {
            string[] ids = new string[100];
            insertNewUser.Parameters["@identifier"].Value = id;
            insertNewUser.Parameters["@password"].Value = pass;
            try
            {
                insertNewUser.Connection.Open();
                int a = insertNewUser.ExecuteNonQuery();
                if (a == 0)
                    MessageBox.Show("Error!");
                insertNewUser.Connection.Close();
            }
            catch (MySqlException y)
            {
                MessageBox.Show(y.Message);
            }
            ids = this.fetchIDs();
            return ids;
        }
        public delegate void messageReceptionHandler(object sender,
            messageReceptionEventArgs eveArgs);
        public class messageReceptionEventArgs : EventArgs
        {
            private byte[] theMessage;
            private bool m_disconnection;
            private Socket m_sock;
            public messageReceptionEventArgs(byte[] m, bool a, Socket s)
            {
                theMessage = m;
                m_disconnection = a;
                m_sock = s;
            }
            public byte[] message
            {
                get
                {
                    return theMessage;
                }
            }
            public bool Disconnection
            {
                get
                {
                    return m_disconnection;
                }
            }
            public Socket theSocket
            {
                get
                {
                    return m_sock;
                }
            }
        }
        public class receivingThread
        {
            public event disconnectionHandler Disconnection;
            public event messageReceptionHandler messageReceived;
            public string messageToSend;
            public Socket sock;
            public void receiving()
            {
                int a = 0;

                for (; ; )
                {
                    byte[] message = new byte[1024];

                    a = sock.Receive(message);
                    if (a == 0)
                    {
                        //MessageBox.Show("The Client was Disconnected!");
                        //in event methode send message ra farakhani mikonad
                        //ke an method sockete morede nazar ra az socketArray
                        //hazf mikonad ve eventi ijad mikonad ke liste user haye
                        //online update shavad
                        messageReceptionEventArgs f = new messageReceptionEventArgs(message, true, sock);
                        messageReceived(this, f);
                        //
                        disconnectionEventArgs po = new disconnectionEventArgs(sock);
                        Disconnection(this, po);

                        break;
                    }
                    else
                    {
                        messageReceptionEventArgs k = new messageReceptionEventArgs(message, false, sock);
                        messageReceived(this, k);
                    }
                }
            }

        }
        public void close()
        {
            Thread temp;
            Socket temp1;
            connThread.Abort();
            foreach (object item in clientThreads)
            {
                temp = (Thread)item;
                temp.Abort();
            }
            sock1.Close();
            foreach (DictionaryEntry item in socketArray)
            {
                temp1 = (Socket)item.Value;
                temp1.Shutdown(SocketShutdown.Both);
                temp1.Close();
            }
        }
        public int port
        {
            set
            {
                m_port = value;
            }
        }
    }
    public class clientConnectionEventArgs : EventArgs
    {
        private string m_id;
        private bool connected;
        public clientConnectionEventArgs(byte[] p)
        {
            m_id = Encoding.ASCII.GetString(p, 0, p.Length);
            connected = false;
        }
        public clientConnectionEventArgs(string y, bool g)
        {
            connected = g;
            m_id = y;
        }
        public string ID
        {
            get
            {
                return m_id;
            }
        }
        public bool Connected
        {
            get
            {
                return connected;
            }
        }
    }
    public class disconnectionEventArgs : EventArgs
    {
        private Socket m_socket;
        public disconnectionEventArgs(Socket socket)
        {
            m_socket = socket;
        }
        public Socket theSocket
        {
            get
            {
                return m_socket;
            }
        }
    }
}
