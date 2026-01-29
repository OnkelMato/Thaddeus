using FluentAssertions;
using NUnit.Framework;
using OnkelMato.Thaddeus.Telegram.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OnkelMato.Thaddeus.Telegram.Tests
{
    [TestFixture]
    public class BotMessageToRequestConverterTests
    {
        #region Termine Tests

        [Test(Description = "Request termine")]
        public void ConvertFullTermineToMessage()
        {
            var message = new Message()
            {
                Text = "Termine 15.8.2024",
                Chat = new Chat { Id = 12345 }
            };
            var sut = new BotMessageToRequestConverter();
            var actual = sut.Convert(message, UpdateType.Message);
            actual.Should().BeOfType<GetAppointmentsRequest>();
            var req = (actual as GetAppointmentsRequest)!;
            req.Date.Should().Be(new DateOnly(2024, 8, 15));
        }

        [Test(Description = "Request termine")]
        public void ConvertNoYearTermineToMessage()
        {
            var now = DateTime.Now;
            var message = new Message()
            {
                Text = "Termine 15.8.",
                Chat = new Chat { Id = 12345 }
            };
            var sut = new BotMessageToRequestConverter();
            var actual = sut.Convert(message, UpdateType.Message);
            actual.Should().BeOfType<GetAppointmentsRequest>();
            var req = (actual as GetAppointmentsRequest)!;
            req.Date.Should().Be(new DateOnly(now.Year, 8, 15));
        }

        [Test(Description = "Request termine")]
        public void ConvertNoMonthTermineToMessage()
        {
            var now = DateTime.Now;
            var message = new Message()
            {
                Text = "Termine 15.",
                Chat = new Chat { Id = 12345 }
            };
            var sut = new BotMessageToRequestConverter();
            var actual = sut.Convert(message, UpdateType.Message);
            actual.Should().BeOfType<GetAppointmentsRequest>();
            var req = (actual as GetAppointmentsRequest)!;
            req.Date.Should().Be(new DateOnly(now.Year, now.Month, 15));
        }

        [Test(Description = "Request termine heute")]
        public void ConvertHeuteTermineToMessage()
        {
            var now = DateTime.Now;
            var message = new Message()
            {
                Text = "Termine heute",
                Chat = new Chat { Id = 12345 }
            };
            var sut = new BotMessageToRequestConverter();
            var actual = sut.Convert(message, UpdateType.Message);
            actual.Should().BeOfType<GetAppointmentsRequest>();
            var req = (actual as GetAppointmentsRequest)!;
            req.Date.Should().Be(new DateOnly(now.Year, now.Month, now.Day));
        }

        [Test(Description = "Request termine gestern")]
        public void ConvertGesternTermineToMessage()
        {
            var now = DateTime.Now.AddDays(-1);
            var message = new Message()
            {
                Text = "Termine gestern",
                Chat = new Chat { Id = 12345 }
            };
            var sut = new BotMessageToRequestConverter();
            var actual = sut.Convert(message, UpdateType.Message);
            actual.Should().BeOfType<GetAppointmentsRequest>();
            var req = (actual as GetAppointmentsRequest)!;
            req.Date.Should().Be(new DateOnly(now.Year, now.Month, now.Day));
        }

        [Test(Description = "Request termine morgen")]
        public void ConvertMorgenTermineToMessage()
        {
            var now = DateTime.Now.AddDays(1);
            var message = new Message()
            {
                Text = "Termine morgen",
                Chat = new Chat { Id = 12345 }
            };
            var sut = new BotMessageToRequestConverter();
            var actual = sut.Convert(message, UpdateType.Message);
            actual.Should().BeOfType<GetAppointmentsRequest>();
            var req = (actual as GetAppointmentsRequest)!;
            req.Date.Should().Be(new DateOnly(now.Year, now.Month, now.Day));
        }

        #endregion

        #region Termin tests

        [Test]
        public void ConvertFullDayTerminToMessage()
        {
            var message = new Message()
            {
                Text = "Termin 15.8.2024 Meeting with team",
                Chat = new Chat { Id = 12345 }
            };

            var sut = new BotMessageToRequestConverter();

            var actual = sut.Convert(message, UpdateType.Message);

            actual.Should().BeOfType<AddAppointmentRequest>();
            var req = actual as AddAppointmentRequest;
            req.Appointment.Start.Should().Be(new DateTime(2024, 8, 15, 0, 0, 0));
            req.Appointment.End.Should().Be(new DateTime(2024, 8, 16, 0, 0, 0));
            req.Appointment.Title.Should().Be("Meeting with team");
        }

        [Test]
        public void ConvertFullTerminToMessage()
        {
            var message = new Message()
            {
                Text = "Termin 15.8.2024 14:30 Meeting with team",
                Chat = new Chat { Id = 12345 }
            };

            var sut = new BotMessageToRequestConverter();

            var actual = sut.Convert(message, UpdateType.Message);

            actual.Should().BeOfType<AddAppointmentRequest>();
            var req = actual as AddAppointmentRequest;
            req.Appointment.Start.Should().Be(new DateTime(2024, 8, 15, 14, 30, 0));
            req.Appointment.End.Should().Be(new DateTime(2024, 8, 15, 15, 0, 0));
            req.Appointment.Title.Should().Be("Meeting with team");
        }

        [Test]
        public void ConvertNoYearTerminToMessage()
        {
            var now = DateTime.Now;
            var message = new Message()
            {
                Text = "Termin 15.8. 14:30 Meeting with team",
                Chat = new Chat { Id = 12345 }
            };

            var sut = new BotMessageToRequestConverter();

            var actual = sut.Convert(message, UpdateType.Message);

            actual.Should().BeOfType<AddAppointmentRequest>();
            var req = (actual as AddAppointmentRequest)!;
            req.Appointment.Start.Should().Be(new DateTime(now.Year, 8, 15, 14, 30, 0));
            req.Appointment.End.Should().Be(new DateTime(now.Year, 8, 15, 15, 0, 0));
            req.Appointment.Title.Should().Be("Meeting with team");
        }

        [Test]
        public void ConvertNoYearMonthTerminToMessage()
        {
            var now = DateTime.Now;
            var message = new Message()
            {
                Text = "Termin 15. 14:30 Meeting with team",
                Chat = new Chat { Id = 12345 }
            };

            var sut = new BotMessageToRequestConverter();

            var actual = sut.Convert(message, UpdateType.Message);

            actual.Should().BeOfType<AddAppointmentRequest>();
            var req = (actual as AddAppointmentRequest)!;
            req.Appointment.Start.Should().Be(new DateTime(now.Year, now.Month, 15, 14, 30, 0));
            req.Appointment.End.Should().Be(new DateTime(now.Year, now.Month, 15, 15, 0, 0));
            req.Appointment.Title.Should().Be("Meeting with team");
        }

        [Test]
        public void ConvertHourOnlyTerminToMessage()
        {
            var message = new Message()
            {
                Text = "Termin 15.8.2024 14 Meeting with team",
                Chat = new Chat { Id = 12345 }
            };

            var sut = new BotMessageToRequestConverter();

            var actual = sut.Convert(message, UpdateType.Message);

            actual.Should().BeOfType<AddAppointmentRequest>();
            var req = (actual as AddAppointmentRequest)!;
            req.Appointment.Start.Should().Be(new DateTime(2024, 8, 15, 14, 00, 0));
            req.Appointment.End.Should().Be(new DateTime(2024, 8, 15, 14, 30, 0));
            req.Appointment.Title.Should().Be("Meeting with team");
        }

        #endregion

        #region Wann tests

        [Test]
        public void ConvertWannIstToMessage()
        {
            var message = new Message()
            {
                Text = "Wann ist Team Meeting",
                Chat = new Chat { Id = 12345 }
            };
            var sut = new BotMessageToRequestConverter();
            var actual = sut.Convert(message, UpdateType.Message);
            actual.Should().BeOfType<FindAppointmentTimeRequest>();
            var req = (actual as FindAppointmentTimeRequest)!;
            req.SearchTerm.Should().Be("Team Meeting");
        }

        #endregion
    }
}
