using System;
using System.Drawing.Printing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Airport.Data.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Data.SqlClient;


namespace Airport.KioskClient
{
    public partial class Form1 : Form
    {

        private HubConnection _hubConnection;
        private string _currentFlightNumber = "";


        private Label? lblPassport;
        private TextBox? txtPassport;
        private Button btnSearch;
        private Label? lblMessage;

        // Шинээр нэмэх Label-ууд
        private Label? lblPassengerId;
        private Label? lblFullName;
        private Label? lblPassportNumber;
        private Label? lblBookingsDetails;
        private ComboBox cmbFlights;

        private Label selectedSeatLabel = null;


        private Button lblCheckInButton;
       
        private String CheckInSeat;
        private readonly HttpClient _httpClient = new HttpClient();

        private ClientWebSocket _wsClient;
        private CancellationTokenSource _wsCts;

        private ListBox socketMessageList;

        public Form1()
        {
            InitializeComponent();
            InitializeWebSocket();
            _hubConnection = new HubConnectionBuilder().WithUrl("http://localhost:5208/flightHub").Build();
            this.AutoScaleMode = AutoScaleMode.None;
            SetupControls();
            //ачааллах үед онгоцны мэдээлэл харуулах async task ажиллах ёстой
            //this.Load += async (s, e) => await LoadFlightsAsync();
            //ачааллах үед онгоцны суудлууд
            //this.Load += async (s, e) => await SetupSeatUIAsync();
            this.Load += async (s, e) =>
            {
                await LoadFlightsAsync();
               // await SetupSeatUIAsync();
                
            };



        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            // WebSocket-ийг хаахын тулд цуцалга хийх
            _wsCts.Cancel();
        }

        private async void InitializeWebSocket()
        {
            _wsClient = new ClientWebSocket();
            _wsCts = new CancellationTokenSource();

            try
            {
                // 1. Серверийн WebSocket URL (жишээ нь)
                var serverUri = new Uri("ws://localhost:5001/ws/");

                // 2. Холбогдох
                await _wsClient.ConnectAsync(serverUri, _wsCts.Token);
                Console.WriteLine("WebSocket connection success.");

                // 3. Клиент талд хүлээн авах цикл (таск дээр ажиллана)
                _ = Task.Run(async () =>
                {
                    var buffer = new byte[4 * 1024];
                    try
                    {
                        while (_wsClient.State == WebSocketState.Open && !_wsCts.IsCancellationRequested)
                        {
                            // 3.1. Серверээс мессеж хүлээн авах
                            var result = await _wsClient.ReceiveAsync(
                                new ArraySegment<byte>(buffer), 
                                _wsCts.Token);

                            if (result.MessageType == WebSocketMessageType.Text)
                            {
                                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                                // 3.2. UI thread дээр MessageBox эсвэл бусад UI update хийх
                                Invoke(new Action(async () =>
                                {
                                    socketMessageList.Items.Add(message);
                                    await SetupSeatUIAsync();
                                }));
                            }
                            else if (result.MessageType == WebSocketMessageType.Close)
                            {
                                // Сервер хаасан бол холболтоо хаах
                                await _wsClient.CloseAsync(
                                    WebSocketCloseStatus.NormalClosure, 
                                    "Close response received", 
                                    CancellationToken.None);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Цуцлагдсан үед энд орно
                    }
                    catch (WebSocketException wsex)
                    {
                        // Алдаа гарсан үед Console эсвэл лог дээр үзүүлнэ
                        Console.WriteLine("WebSocket error: " + wsex.Message);
                    }
                }, _wsCts.Token);
            }
            catch (Exception ex)
            {
                MessageBox.Show("WebSocket connection error: " + ex.Message);
            }
        }



        // бүртгэл хийгдсэн үед товч байхгүй, хийгдээгүй үед товч гарах
        private void CheckInConsoleMessage(bool checkedIn)
        {
            if (checkedIn)
            {
                this.Controls.Remove(lblCheckInButton);
            }
            else
            {
                this.Controls.Add(lblCheckInButton);
            }
        }
        private int MpassengerId;
        // search button дарагдах үед
        private async void BtnSearch_Click(object? sender, EventArgs e)
        {

           
            string passport = txtPassport!.Text.Trim();
            if (string.IsNullOrEmpty(passport))
            {
                lblMessage!.Text = "Please enter a passport number.";
                return;
            }

            lblMessage!.Text = "Searching...";
            lblPassengerId!.Text = "";
            lblFullName!.Text = "";
            lblPassportNumber!.Text = "";

            try
            {
                string url = $"http://localhost:5208/api/passengers/search?passport={passport}";
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    lblMessage.Text = "Passenger not found.";
                    return;
                }

                string json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                MpassengerId = root.GetProperty("passengerId").GetInt32();
                string fullName = root.GetProperty("fullName").GetString() ?? "";
                string passportNumber = root.GetProperty("passportNumber").GetString() ?? "";

                lblPassengerId.Text = $"Passenger ID: {MpassengerId}";
                lblFullName.Text = $"Full Name: {fullName}";
                lblPassportNumber.Text = $"Passport Number: {passportNumber}";

                if (!root.TryGetProperty("bookings", out var bookingsElement) || bookingsElement.GetArrayLength() == 0)
                {
                    lblMessage.Text = "No bookings found in the response.";
                    return;
                }

                var bookings = bookingsElement.EnumerateArray()
                    .Select(b => new
                    {
                        BookingId = b.GetProperty("bookingId").GetInt32(),
                        FlightNumber = b.GetProperty("flightNumber").GetString(),
                        DepartureTime = b.GetProperty("departureTime").GetDateTime(),
                        CheckedIn = b.GetProperty("checkedIn").GetBoolean()
                    }).ToList();

                lblMessage.Text = $"Found {bookings.Count} booking(s) for {fullName}";
                lblBookingsDetails.Text = string.Join(Environment.NewLine, bookings.Select(b =>
                                     $"Booking ID: {b.BookingId}, Flight: {b.FlightNumber}, " +
                                     $"Departure: {b.DepartureTime}, Checked In: {(b.CheckedIn ? "Yes" : "No")}"));

                CheckInSeat = bookings.FirstOrDefault()?.FlightNumber ?? "0";
                await SetupSeatUIAsync();
                foreach (var b in bookings)
                {
                    CheckInConsoleMessage(b.CheckedIn);
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error: {ex.Message}";
            }
        }



        // онгоцны мэдээлэл харуулах asynctask
        private async Task LoadFlightsAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync("http://localhost:5208/api/flights");
                    response.EnsureSuccessStatusCode();
                    var json = await response.Content.ReadAsStringAsync();
                    var flights = JsonSerializer.Deserialize<List<Flight>>(json);

                    if (flights != null)
                    {
                        cmbFlights.DataSource = flights;
                        cmbFlights.DisplayMember = "flightNumber";
                        cmbFlights.ValueMember = "flightId";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        //Суудал харуулах
        private async Task<List<Seat>> LoadSeatsAsync()
        {
            //Value member нь нислэгийн ID
            String flightNumber = CheckInSeat;
            using var client = new HttpClient();
           
            string url = $"http://localhost:5208/api/seats/by-flight/{flightNumber}";

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Seat>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return new List<Seat>();
        }

        //suudliin label click hiih handler
        private void SeatLabel_Click(object sender, EventArgs e)//TODO buruu ongotsnii sudal songood baiga
        {
            if (sender is Label lbl && lbl.Tag is string seatNumber)
            {
                // Жишээ нь: MessageBox-оор харуулах
                MessageBox.Show($"You selected seat: {seatNumber}", "Seat Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (selectedSeatLabel != null)
                {
                    // Хуучин сонгосон суудал нь эзлэгдсэн эсэхээс хамааран өнгийг нь сэргээх
                    bool wasOccupied = selectedSeatLabel.BackColor == Color.LightCoral;
                    selectedSeatLabel.BackColor = wasOccupied ? Color.LightCoral : Color.LightGreen;
                }

                // Одоогийн сонгосон суудлыг саарал болгох
                lbl.BackColor = Color.Gray;
                selectedSeatLabel = lbl;


                // Эсвэл өөр TextBox-д харуулах:
                // txtSelectedSeat.Text = seatNumber;
            }
        }


        // flightId-аар(cmbFlights.ValueMember) суудал table байдлаар харуулах
        private async Task SetupSeatUIAsync()
        {
            var seats = await LoadSeatsAsync();

            // 2.2. Өмнөх сонгогдсон суудлыг цэвэрлэх
            selectedSeatLabel = null;

            // 2.1. Хэрвээ өмнөх seatContainer байгаа бол устгах
            var existingPanel = this.Controls.OfType<Panel>()
                               .FirstOrDefault(p => p.Name == "seatContainer");
            if (existingPanel != null)
            {
                this.Controls.Remove(existingPanel);
                existingPanel.Dispose();
            }

            // 2.3. Шинэ Panel үүсгэх
            var seatContainer = new Panel
            {
                Name = "seatContainer",
                Left = 470,
                Top = 30,
                Width = 440,
                Height = 400,
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle
            };

            // 2.4. TableLayoutPanel үүсгэх
            var table = new TableLayoutPanel
            {
                RowCount = 30,
                ColumnCount = 6,
                Dock = DockStyle.Top,
                AutoSize = true
            };
            for (int c = 0; c < 6; c++)
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));
            for (int r = 0; r < 30; r++)
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            // 2.5. Суудлуудыг байрлуулах
            for (int row = 1; row <= 30; row++)
            {
                for (int col = 0; col < 6; col++)
                {
                    string seatNumber = $"{row}{(char)('A' + col)}";
                    var seat = seats.FirstOrDefault(s => s.SeatNumber == seatNumber);

                    var lblSeat = new Label
                    {
                        Text = seatNumber,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Fill,
                        Margin = new Padding(2),
                        BackColor = (seat != null && seat.IsOccupied)
                            ? Color.LightCoral
                            : Color.LightGreen,
                        Tag = seatNumber,
                        Cursor = Cursors.Hand
                    };

                    // 2.6. Click эвент холбох
                    lblSeat.Click += SeatLabel_Click;

                    table.Controls.Add(lblSeat, col, row - 1);
                }
            }

            seatContainer.Controls.Add(table);
            this.Controls.Add(seatContainer);
        }


        //Check in хийх process
        private async void LblCheckIn_Click(object sender, EventArgs e)
        {
            // 1. Сонгосон суудлын дугаар байгаа эсэхийг шалгах
            if (selectedSeatLabel == null || !(selectedSeatLabel.Tag is string seatNumber))
            {
                MessageBox.Show("Suudlaa songo.", "FF", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Зорчигчийн ID-г Label-аас зөвшөөрөгдөхөөр авч чаддаг болгох (Жишээ: "2" гэсэн тоог шууд харуулсан гэж тооцно)
            //if (!int.TryParse(MpassengerId, out int passengerId))
            //{
            //    MessageBox.Show("Зорчигчийн ID буруу байна.", "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}

            // 3. Нислэгийн ID-г ComboBox.SelectedValue-аас авч чаддаг болгох
            //if (!int.TryParse(cmbFlights.SelectedValue?.ToString(), out int flightId)) // TODO nislegiig databasees avah
            //{
            //    MessageBox.Show("Nisleg songogdoogui.", "FF", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            


            int flightId = 0;
            try
            {
                string connectionString = @"Data Source=C:\Users\sssin\source\repos\AirportCheckIn\src\Airport.Server\airport.db";
                using (var connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new Microsoft.Data.Sqlite.SqliteCommand("SELECT FlightId FROM Flights WHERE FlightNumber = @CheckInSeat", connection))
                    {
                        command.Parameters.AddWithValue("@CheckInSeat", CheckInSeat);
                        var result = command.ExecuteScalar();
                        if (result != null && int.TryParse(result.ToString(), out flightId))
                        {
                            // Flight ID found
                        }
                        else
                        {
                            MessageBox.Show("Nislegiin medeelel oldsongui.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database aldaa: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using var client = new HttpClient();

                // 4. Зорчигч өмнө нь check-in хийсэн эсэхийг API-аар шалгах
                var checkUrl = $"http://localhost:5208/api/seats/checked-in/{MpassengerId}";
                var checkResponse = await client.GetAsync(checkUrl);

                if (checkResponse.IsSuccessStatusCode)
                {
                    var json = await checkResponse.Content.ReadAsStringAsync();
                    bool alreadyCheckedIn = JsonSerializer.Deserialize<bool>(json);

                    if (alreadyCheckedIn)
                    {
                        MessageBox.Show(
                            "Ali hediin suudal songoson baina.",
                            "HHH",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                        return;
                    }
                }
                else
                {
                    MessageBox.Show(
                        $"Check-in shalgaltiin alda. Статус код: {checkResponse.StatusCode}",
                        "Aldaa",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                // 5. Шаардлага тэнцсэн тул суудлыг захиалах (PUT үлдээх API дуудах)
                var seatToUpdate = new Seat
                {
                    SeatNumber = seatNumber,
                    FlightId = flightId,
                    IsOccupied = true,
                    PassengerId = MpassengerId
                };

                var jsonBody = JsonSerializer.Serialize(seatToUpdate);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var url = "http://localhost:5208/api/seats/seat-status";

                var updateResponse = await client.PutAsync(url, content);
                if (updateResponse.IsSuccessStatusCode)
                {
                    var result = MessageBox.Show(
                       "Suudal amjilttai songogdov",
                       "Amjilttai",
                       MessageBoxButtons.OK,
                       MessageBoxIcon.Information
                   );

                    if (result == DialogResult.OK)
                    {
                        PrintBoardingPass(); 

                        await SetupSeatUIAsync();
                    }

                    // Шинэчлэгдсэн суудлын UI-г шинэчлэх (үргэлжлүүлэн харуулах эсвэл дахин сэргээх г.м.)
                    await SetupSeatUIAsync();
                }
                else
                {
                    MessageBox.Show(
                        $"Suudal burtegehed aldaa garav. : {updateResponse.StatusCode}",
                        "Aldaa",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $" Trycatch err {ex.Message}",
                    "Aldaa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void PrintBoardingPass()
        {

            // zorchigchiin medeelliig oruulad mashiniin systemiin hevleh functseer damjuulna
            PrintDocument printDoc = new PrintDocument();
            printDoc.PrintPage += (sender, e) =>
            {
                e.Graphics.DrawString("Boarding Pass", new Font("Arial", 16), Brushes.Black, new PointF(100, 100));
                e.Graphics.DrawString("Flight: AB123", new Font("Arial", 12), Brushes.Black, new PointF(100, 140));
                e.Graphics.DrawString("Seat: 12A", new Font("Arial", 12), Brushes.Black, new PointF(100, 170));
            };

            try
            {
                printDoc.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hevleh hesgiin aldaa: " + ex.Message);
            }
        }

    }

}
