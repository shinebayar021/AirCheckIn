using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Airport.Data.Models;

namespace Airport.KioskClient
{
    public partial class Form1 : Form
    {

        private void SetupControls()
        {

            this.Text = "Passenger Search by Passport";
            this.Width = 500;
            this.Height = 450;
            this.StartPosition = FormStartPosition.CenterScreen;

            lblBookingsDetails = new Label()
            {
                Left = 10,
                Top = 110,
                Width = 460,
                Height = 80,
                ForeColor = System.Drawing.Color.Black,
                AutoSize = false
            };

            lblPassport = new Label()
            {
                Text = "Passport Number:",
                Left = 10,
                Top = 20,
                Width = 110
            };

            txtPassport = new TextBox()
            {
                Left = 130,
                Top = 18,
                Width = 200
            };

            btnSearch = new Button()
            {
                Text = "Search",
                Left = 340,
                Top = 16,
                Width = 80,
                Height = 30
            };
            btnSearch.Click += BtnSearch_Click;



            lblMessage = new Label()
            {
                Left = 10,
                Top = 370,
                Width = 460,
                ForeColor = System.Drawing.Color.Blue
            };

            lblPassengerId = new Label()
            {
                Left = 10,
                Top = 50,
                Width = 460,
                ForeColor = System.Drawing.Color.Black
            };

            lblFullName = new Label()
            {
                Left = 10,
                Top = 70,
                Width = 460,
                ForeColor = System.Drawing.Color.Black
            };

            lblPassportNumber = new Label()
            {
                Left = 10,
                Top = 90,
                Width = 460,
                ForeColor = System.Drawing.Color.Black
            };

            lblCheckInButton = new Button()
            {
                Text = "CheckIn",
                Left = 340,
                Top = 190,
                Width = 80,
                Height = 40
            };
            lblCheckInButton.Click += LblCheckIn_Click;


            ComboBox cmbFlightStatus = new ComboBox()
            {
                Left = 10,
                Top = 240,
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFlightStatus.Items.AddRange(Enum.GetNames(typeof(FlightStatus)));
            cmbFlightStatus.SelectedIndex = 0;
            this.Controls.Add(cmbFlightStatus);

            socketMessageList = new ListBox()
            {
                Left = 10,
                Top = 290,
                Width = 400,
                HorizontalScrollbar = true
            };

            Button btnUpdateStatus = new Button()
            {
                Text = "Update Flight Status",
                Left = 220,
                Top = 240,
                Width = 180,
                Height = 30
            };
            this.Controls.Add(btnUpdateStatus);
            // төлөв өөрчлөх eventhandler
            btnUpdateStatus.Click += async (sender, e) =>
            {

                // Сонгогдсон онгоцны FlightNumber авах
                var selectedFlight = (Flight)cmbFlights.SelectedItem;
                string flightNumber = selectedFlight.FlightNumber;

                // FlightStatus enum-аас сонгогдсон статусыг авах
                string selectedStatusName = cmbFlightStatus.SelectedItem?.ToString() ?? "";
                if (!Enum.TryParse<FlightStatus>(selectedStatusName, out var selectedStatus))
                {
                    MessageBox.Show("Invalid flight status selected.");
                    return;
                }

                try
                {
                    string url = $"http://localhost:5208/api/flights/update-status/{flightNumber}";

                    // PUT хүсэлтээр enum-ын int утгыг JSON-р илгээх
                    HttpResponseMessage response = await _httpClient.PutAsJsonAsync(url, (int)selectedStatus);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"Flight '{flightNumber}' status амжилттай шинэчлэгдлээ.");
                        // Шинэчлэгдсэн мэдээллийг дахин ачаалж харуулах
                        await LoadFlightsAsync();
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Алдаа гарлаа: {errorContent}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Алдаа гарлаа: " + ex.Message);
                }
            };

            cmbFlights = new ComboBox()
            {
                Left = 10,
                Top = 150,
                Width = 400,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.Controls.Add(cmbFlights);

            // Онгоцны төлвийг status combobox дээрээ хадгалдаг байх
            cmbFlights.SelectedIndexChanged += (s, e) =>
            {
                if (cmbFlights.SelectedItem is Flight selectedFlight)
                {
                    cmbFlightStatus.SelectedItem = selectedFlight.Status.ToString();
                }
            };

           

            this.Controls.Add(lblPassport);
            this.Controls.Add(txtPassport);
            this.Controls.Add(btnSearch);
            this.Controls.Add(lblMessage);
            this.Controls.Add(lblPassengerId);
            this.Controls.Add(lblFullName);
            this.Controls.Add(lblPassportNumber);
            this.Controls.Add(lblBookingsDetails);

            this.Controls.Add(socketMessageList);


        }
    }
}
