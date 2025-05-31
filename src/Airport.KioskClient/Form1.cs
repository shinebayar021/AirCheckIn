using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Airport.Data.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace Airport.KioskClient
{
    public partial class Form1 : Form
    {

        HubConnection hubConnection;

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

        public Form1()
        {
            InitializeComponent();
            hubConnection = new HubConnectionBuilder().WithUrl("http://localhost:5208/flightHub").Build();
            this.AutoScaleMode = AutoScaleMode.None;
            SetupControls();
            //ачааллах үед онгоцны мэдээлэл харуулах async task ажиллах ёстой
            this.Load += async (s, e) => await LoadFlightsAsync();
            //ачааллах үед онгоцны суудлууд
            this.Load += async (s, e) => await SetupSeatUIAsync();

        }

        private void CheckInButtonClick()
        {

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
                        SeatNumber = b.GetProperty("seatNumber").GetString(),
                        CheckedIn = b.GetProperty("checkedIn").GetBoolean()
                    }).ToList();

                lblMessage.Text = $"Found {bookings.Count} booking(s) for {fullName}";
                lblBookingsDetails.Text = string.Join(Environment.NewLine, bookings.Select(b =>
                                     $"Booking ID: {b.BookingId}, Flight: {b.FlightNumber}, Seat: {b.SeatNumber}, " +
                                     $"Departure: {b.DepartureTime}, Checked In: {(b.CheckedIn ? "Yes" : "No")}"));

                CheckInSeat = bookings.FirstOrDefault()?.FlightNumber ?? "0";

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
                MessageBox.Show("Алдаа гарлаа: " + ex.Message);
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
        private void SeatLabel_Click(object sender, EventArgs e)
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

            // Хэрвээ өмнөх seatContainer байгаа бол устгах
            var existingPanel = this.Controls.OfType<Panel>()
                               .FirstOrDefault(p => p.Name == "seatContainer");
            if (existingPanel != null)
            {
                this.Controls.Remove(existingPanel);
                existingPanel.Dispose();
            }

            // Шинэ Panel үүсгэх
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

            // TableLayoutPanel үүсгэх
            var table = new TableLayoutPanel
            {
                RowCount = 30,
                ColumnCount = 6,
                Dock = DockStyle.Top,
                AutoSize = true
            };
            // Баган бүрийн өргөн
            for (int c = 0; c < 6; c++)
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));
            // Мөр бүрийн өндөр
            for (int r = 0; r < 30; r++)
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            // Суудлуудыг байрлуулах
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
                        // Үл ашиглагдаагүй суудал: ногоон, эзлэгдсэн: улаан
                        BackColor = (seat != null && seat.IsOccupied)
                            ? Color.LightCoral
                            : Color.LightGreen,
                        // Tag-д суудлын дугаар хадгалах
                        Tag = seatNumber,
                        Cursor = Cursors.Hand // mouse pointer-г өөрчлөх
                    };


                    // Click эвент холбох
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
                MessageBox.Show("Суудлаа сонгоно уу.", "Анхаар", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Зорчигчийн ID-г Label-аас зөвшөөрөгдөхөөр авч чаддаг болгох (Жишээ: "2" гэсэн тоог шууд харуулсан гэж тооцно)
            //if (!int.TryParse(MpassengerId, out int passengerId))
            //{
            //    MessageBox.Show("Зорчигчийн ID буруу байна.", "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}

            // 3. Нислэгийн ID-г ComboBox.SelectedValue-аас авч чаддаг болгох
            if (!int.TryParse(cmbFlights.SelectedValue?.ToString(), out int flightId))
            {
                MessageBox.Show("Нислэг сонгогдоогүй байна.", "Анхаар", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                            "Та аль хэдийн суудал захиалсан байна. Дахин суудал сонгох боломжгүй.",
                            "Мэдэгдэнэ",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                        return;
                    }
                }
                else
                {
                    MessageBox.Show(
                        $"Check-in шалгалт хийхэд алдаа гарлаа. Статус код: {checkResponse.StatusCode}",
                        "Алдаа",
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
                    MessageBox.Show(
                        "Суудал амжилттай захиалагдлаа!",
                        "Амжилттай",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    // Шинэчлэгдсэн суудлын UI-г шинэчлэх (үргэлжлүүлэн харуулах эсвэл дахин сэргээх г.м.)
                    await SetupSeatUIAsync();
                }
                else
                {
                    MessageBox.Show(
                        $"Суудал шинэчлэхэд алдаа гарлаа. Статус код: {updateResponse.StatusCode}",
                        "Алдаа",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Харилцааны алдаа гарлаа: {ex.Message}",
                    "Алдаа",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }



    }

}
