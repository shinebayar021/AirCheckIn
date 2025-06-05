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
            // Үндсэн форм тохиргоо
            this.Text = "Нисэх онгоцны буудлын киоск - Зорчигч бүртгэх систем";
            this.Width = 1000;
            this.Height = 700;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 248, 255); // Цайвар цэнхэр өнгө
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            // Гарчиг панел
            Panel headerPanel = new Panel()
            {
                Left = 0,
                Top = 0,
                Width = this.Width,
                Height = 80,
                BackColor = Color.FromArgb(70, 130, 180),
                Dock = DockStyle.Top
            };

            Label headerLabel = new Label()
            {
                Text = "✈️ КИОСК",
                Left = 20,
                Top = 20,
                Width = 600,
                Height = 40,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            };

            headerPanel.Controls.Add(headerLabel);
            this.Controls.Add(headerPanel);

            // Зорчигч хайх хэсэг
            GroupBox searchGroup = new GroupBox()
            {
                Text = "Зорчигчийн мэдээлэл хайх",
                Left = 20,
                Top = 100,
                Width = 480,
                Height = 180,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(70, 130, 180)
            };

            lblPassport = new Label()
            {
                Text = "Паспортын дугаар:",
                Left = 20,
                Top = 30,
                Width = 140,
                Height = 25,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(60, 60, 60)
            };

            txtPassport = new TextBox()
            {
                Left = 20,
                Top = 55,
                Width = 250,
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle
            };

            btnSearch = new Button()
            {
                Text = "🔍 Хайх",
                Left = 280,
                Top = 53,
                Width = 100,
                Height = 35,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += BtnSearch_Click;

            // Зорчигчийн мэдээлэл харуулах хэсэг
            lblPassengerId = new Label()
            {
                Left = 20,
                Top = 95,
                Width = 440,
                Height = 20,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(40, 40, 40)
            };

            lblFullName = new Label()
            {
                Left = 20,
                Top = 115,
                Width = 440,
                Height = 20,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40)
            };

            lblPassportNumber = new Label()
            {
                Left = 20,
                Top = 135,
                Width = 440,
                Height = 20,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(40, 40, 40)
            };

            searchGroup.Controls.AddRange(new Control[] { lblPassport, txtPassport, btnSearch, lblPassengerId, lblFullName, lblPassportNumber });
            this.Controls.Add(searchGroup);

            // Захиалгын мэдээлэл хэсэг
            GroupBox bookingGroup = new GroupBox()
            {
                Text = "Захиалгын мэдээлэл",
                Left = 20,
                Top = 290,
                Width = 480,
                Height = 120,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(70, 130, 180)
            };

            lblBookingsDetails = new Label()
            {
                Left = 15,
                Top = 25,
                Width = 450,
                Height = 85,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(40, 40, 40),
                AutoSize = false,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Padding = new Padding(5)
            };

            bookingGroup.Controls.Add(lblBookingsDetails);
            this.Controls.Add(bookingGroup);

            // Нислэгийн удирдлага хэсэг
            GroupBox flightManagementGroup = new GroupBox()
            {
                Text = "Нислэгийн төлөв өөрчлөх",
                Left = 20,
                Top = 420,
                Width = 480,
                Height = 100,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(70, 130, 180)
            };

            cmbFlights = new ComboBox()
            {
                Left = 15,
                Top = 25,
                Width = 200,
                Height = 25,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };

            ComboBox cmbFlightStatus = new ComboBox()
            {
                Left = 15,
                Top = 55,
                Width = 200,
                Height = 25,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };
            cmbFlightStatus.Items.AddRange(new string[] { "Registering", "Boarding", "Departed", "Delayed", "Cancelled" });
            cmbFlightStatus.SelectedIndex = 0;

            Button btnUpdateStatus = new Button()
            {
                Text = "Төлөв шинэчлэх",
                Left = 230,
                Top = 40,
                Width = 120,
                Height = 35,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnUpdateStatus.FlatAppearance.BorderSize = 0;

            // Төлөв өөрчлөх event handler (танай бичсэн код)
            btnUpdateStatus.Click += async (sender, e) =>
            {
                var selectedFlight = (Flight)cmbFlights.SelectedItem;
                if (selectedFlight == null) return;

                string flightNumber = selectedFlight.FlightNumber;
                string selectedStatusName = cmbFlightStatus.SelectedItem?.ToString() ?? "";

                if (!Enum.TryParse<FlightStatus>(selectedStatusName, out var selectedStatus))
                {
                    MessageBox.Show("Буруу төлөв сонгосон байна.", "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    string url = $"http://localhost:5208/api/flights/update-status/{flightNumber}";
                    HttpResponseMessage response = await _httpClient.PutAsJsonAsync(url, (int)selectedStatus);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"'{flightNumber}' нислэгийн төлөв амжилттай шинэчлэгдлээ.", "Амжилттай", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadFlightsAsync();
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Алдаа гарлаа: {errorContent}", "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Алдаа гарлаа: " + ex.Message, "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // Нислэг сонголт өөрчлөгдөх үед төлвийг шинэчлэх
            cmbFlights.SelectedIndexChanged += (s, e) =>
            {
                if (cmbFlights.SelectedItem is Flight selectedFlight)
                {
                    cmbFlightStatus.SelectedItem = selectedFlight.Status.ToString();
                }
            };

            flightManagementGroup.Controls.AddRange(new Control[] { cmbFlights, cmbFlightStatus, btnUpdateStatus });
            this.Controls.Add(flightManagementGroup);

            // Check-in товч
            lblCheckInButton = new Button()
            {
                Text = "✈️ СУУДАЛ СОНГОЖ БҮРТГҮҮЛЭХ",
                Left = 20,
                Top = 540,
                Width = 200,
                Height = 50,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(255, 140, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            lblCheckInButton.FlatAppearance.BorderSize = 0;
            lblCheckInButton.Click += LblCheckIn_Click;

            // WebSocket мэдээлэл хэсэг
            GroupBox socketGroup = new GroupBox()
            {
                Text = "Системийн мэдээлэл",
                Left = 20,
                Top = 600,
                Width = 480,
                Height = 80,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(70, 130, 180)
            };

            socketMessageList = new ListBox()
            {
                Left = 15,
                Top = 25,
                Width = 450,
                Height = 45,
                Font = new Font("Segoe UI", 8F),
                HorizontalScrollbar = true,
                BackColor = Color.FromArgb(248, 248, 248)
            };

            socketGroup.Controls.Add(socketMessageList);
            this.Controls.Add(socketGroup);

            // Статус мэдээлэл
            lblMessage = new Label()
            {
                Left = 250,
                Top = 570,
                Width = 250,
                Height = 20,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(70, 130, 180),
                Text = "Систем бэлэн байна..."
            };

            // Бүх контролыг нэмэх
            //this.Controls.AddRange(new Control[] { lblCheckInButton, lblMessage });
            this.Controls.Add(lblMessage);

            // Суудлыг харуулах талбар (SetupSeatUIAsync функц энд суудлуудыг харуулна)
            // Энэ хэсэг SetupSeatUIAsync() функцэд тул энд зөвхөн байрлалыг тодорхойлж өгнө
            // Left: 520, Top: 100, Width: 440, Height: 580 хэмжээтэй талбар
        }
    }
}