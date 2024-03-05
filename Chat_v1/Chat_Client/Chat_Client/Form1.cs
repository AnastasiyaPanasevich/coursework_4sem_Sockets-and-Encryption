using System;
using System.Drawing;
using System.Drawing.Text;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Security.Cryptography.RSACryptoServiceProvider;
using MyRSALibrary_v1;
using System.Security.Cryptography;
using System.Timers;
using MySubstitutionCipherLibrary;

namespace Chat_Client
{
    public partial class Form1 : Form
    {
        private bool isDarkTheme = false;
        public bool shown = false;
        private TcpClient client;
        private Thread listeningThread;
        private bool isListening = false;
        private string userName;

        SubstitutionCipher cipher = new SubstitutionCipher();


        public Form1(TcpClient _client, string _userName)
        {
            InitializeComponent();

            buttonGetKeys.Visible = false;


            this.client = _client;
            userName = _userName;
            labelNickname.Text = userName;
            richTextBoxChat.Text = "вы присоединились к чату\n";


            // Запуск прослушивания сервера при загрузке формы
            _ = StartListeningThread();
        }
        private void TimerElapsed(object sender, ElapsedEventArgs e)
{
    // Отправка сообщения "U;" на сервер
    SendMessageToServer("U;");
}


        private void DarkThemeButton_Click(object sender, EventArgs e)
        {
            if (isDarkTheme)
            {
                // Возврат к светлой теме

                this.BackColor = Color.Thistle;
                this.ForeColor = Color.Black;

                DarkThemeButton.Text = "тёмная тема";

                // Изменение цвета кнопок обратно на светлый

                foreach (Control control in this.Controls)
                {
                    if (control is Button)
                    {
                        Button button = (Button)control;
                        button.BackColor = Color.White;
                        button.ForeColor = Color.Indigo;
                    }
                    else if (control is RichTextBox)
                    {
                        RichTextBox richTextBox = (RichTextBox)control;
                        richTextBox.BackColor = Color.White;
                        richTextBox.ForeColor = Color.Purple;
                    }
                }
            }
            else
            {
                // Переход к темной теме
                this.BackColor = Color.FromArgb(0, 58, 68);
                this.ForeColor = Color.White;

                DarkThemeButton.Text = "светлая тема";

                // Изменение цвета кнопок на темный
                foreach (Control control in this.Controls)
                {
                    if (control is Button)
                    {
                        Button button = (Button)control;
                        button.BackColor = Color.FromArgb(39, 82, 90);
                        button.ForeColor = Color.FromArgb(108, 193, 208);
                    }
                    else if (control is RichTextBox)
                    {
                        RichTextBox richTextBox = (RichTextBox)control;
                        richTextBox.BackColor = Color.FromArgb(47, 83, 90);
                        richTextBox.ForeColor = Color.FromArgb(146, 167, 171);
                    }
                }
            }

            // Инвертирование состояния темы
            isDarkTheme = !isDarkTheme;
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            richTextBoxMessage.Text = string.Empty;
        }

        private void CloseConnection()
        {
            try
            {
                // Закрытие потока сети и клиента
                client.GetStream().Close();
                client.Close();
            }
            catch (Exception ex)
            {
                // Обработка исключений, возникающих при закрытии соединения
                MessageBox.Show("Ошибка при закрытии соединения с сервером: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HandleServerResponse(string response)
        {
            if (InvokeRequired)
            {
                Invoke((Action)(() => HandleServerResponse(response)));
                return;
            }

            richTextBoxChat.AppendText(response + Environment.NewLine);
            richTextBoxChat.ScrollToCaret();
        }

        public static string ReplaceSemicolon(string message)
        {
            if (message.Contains(";"))
            {
                message = message.Replace(";", ".");
            }
            return message;
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            // Получение текста из richTextBoxMessage
            string message = richTextBoxMessage.Text;

            message = ReplaceSemicolon(message);

            message = cipher.Encrypt(message);

            // Отправка сообщения на сервер
            SendMessageToServer(message);
        }

        //поиск слова при нажатии клавиши 'Enter'
        private void richTextBoxMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                buttonSend.PerformClick();
            }
        }

        private void SendMessageToServer(string _message)
        {
            try
            {
                string command = "M";
                string message = _message;

                // Combine the command and encrypted message
                string combinedMessage = $"{command};{message}";

                // Convert the combined message to bytes
                byte[] buffer = Encoding.UTF8.GetBytes(combinedMessage);

                // Get the network stream from the TcpClient
                NetworkStream stream = client.GetStream();

                // Send the encrypted message to the server
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();

                //}

                // Clear the richTextBoxMessage after sending the messages
                richTextBoxMessage.Text = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при отправке сообщения: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task StartListeningThread()
        {
            string command, message;
            try
            {
                // Получение потока сети из объекта TcpClient
                NetworkStream stream = client.GetStream();

                // Бесконечный цикл для прослушивания сервера
                while (true)
                {
                    byte[] buffer = new byte[client.ReceiveBufferSize];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        // Сервер отключился
                        break;
                    }

                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    

                    // Разделение сообщения на команду и текст
                    string[] parts = response.Split(';');

                    command = parts[0]; 

                    switch (command)
                    {

                        case "U":

                            message = parts[1];
                            // Обработка ответа сервера
                            //message = cipher.Decrypt(message);

                            label4.Text = message;
                            break;
                        case "J":

                            message = parts[1];
                            string message2 = parts[2];
                            string wholeMsg = message + message2;
                            // Обработка ответа сервера

                            HandleServerResponse(wholeMsg);
                            break;
                        case "M":
                            //buttonGetKeys_Click(sender, new EventArgs());
                            string message1 = parts[1];
                            message = parts[2];
                            message = cipher.Decrypt(message);
                            string wholeMessage = message1 + message;

                            // Обработка ответа сервера
                            HandleServerResponse(wholeMessage);
                            break;
                        default:
                            break;
                    }
                    // Очистка буфера
                    Array.Clear(buffer, 0, buffer.Length);
                }
                // Выполняем необходимые действия после отключения от сервера
                CloseConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении сообщения от сервера: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void StopListeningThread()
        {
            // Устанавливаем флаг прослушивания в false
            isListening = false;

            // Дожидаемся завершения потока
            if (listeningThread != null && listeningThread.IsAlive)
            {
                listeningThread.Join();
                listeningThread = null;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            try
            {

                // Получение потока сети из объекта TcpClient
                NetworkStream stream = client.GetStream();
                // Преобразование сообщения в массив байтов
                byte[] buffer = Encoding.UTF8.GetBytes($"E;{userName}");

                // Отправка сообщения на сервер
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при отправке логина: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            buttonExitChat_Click(sender, e);
        }

        private void buttonExitChat_Click(object sender, EventArgs e)
        {
            // Остановка прослушивания при закрытии формы
            StopListeningThread();

            // Завершение работы приложения
            Application.Exit();
        }


        ////////////////////////////////////////////////////////////////////////////////
        private void buttonGetKeys_Click(object sender, EventArgs e)
        {
            //// Получение потока сети из объекта TcpClient
            //NetworkStream stream = client.GetStream();
            //// Преобразование сообщения в массив байтов
            //byte[] buffer = Encoding.UTF8.GetBytes($"GetUsersKeys;");
            //// Отправка сообщения на сервер
            //stream.Write(buffer, 0, buffer.Length);
            //stream.Flush();
        }
        ////////////////////////////////////////////////////////////////////////////////
    }
}
