using System.ComponentModel;
using Microsoft.SemanticKernel;
using System.Diagnostics;
using MAUIBC;
using System.Globalization;

/// <summary>
/// NFTService provides a set of Bookings entry and booking enquiry functions.
/// </summary>
namespace ChatMAUI;
public sealed class NFTPlugin : INFTPlugin {
	private INFTService? nftService { get; set; }
	public NFTPlugin(INFTService nftservice) {
		nftService = nftservice;
	}
	/// <summary>
	/// NFTService provides a set of Bookings entry and booking enquiry functions.
	/// Customer place order to purchase a certain amount of an item
	/// </summary>

	/// <param name="Date"> Date to book</param>
	/// <returns> Result of booking, either Success or Failed. </returns>
	[KernelFunction, Description("Make a booking for a date")]
    public string MakeBooking(
        [Description("The date to make booking")] string Date) {
		DateTime date = DateTime.ParseExact(Date, "dd/MM/yyyy", CultureInfo.InvariantCulture);

		if (date < DateTime.UtcNow.Date) {
			return $"Failed: Booking must be on a future date!";
		}

		if (!nftService.IsConnected) { return "Please connect your Wallet first!"; }

		string message = "";
		try {
			NFTItem query = nftService.Lookup(date).Result;
			if (query.Id != 0) {
				if (query.Owner == nftService.MyAddress) {
					message = $"Failed: You have already booked {date}";
					if (query.Vacant) {
						message += " but it is vacant. Do you want to Reclaim it instead?";
					}
				} else {
					message = $"Failed: Booking on {date} is currently owned by {query.Owner}.";
					if (query.Vacant) {
						message += " But you can request the owner to tranfer it to you.";
					}
				}
				return message;
			}
			MintEvent mintevent = nftService.Mint(date).Result;
			message = @$"Success: Booking on {mintevent.BookingDate.ToString("dd/MM/yyyy")} confirmed. Please import this NFT into your wallet with Contract:{nftService.contract.Address} and Id:{mintevent.TokenId}";
		} catch (Exception ex) {
			message = ex.Message;
		}
		return message;
	}

	/// <param name="Id"> The Booking Id to Reclaim</param>
	/// <returns> Result of Reclaiming, either Success or Failed. </returns>
	[KernelFunction, Description("Reclaim a booking Id")]
	public string ReclaimBooking(
		[Description("The Booking Id to Reclaim")] int Id) {

		if (!nftService.IsConnected) { return "Please connect your Wallet first!"; }

		try {
			NFTItem result = nftService.Reclaim(Id).Result;
			return $"Success: {result.BookingDate.ToString("dd/MM/yyyy")} Reclaimed.";
		} catch (Exception ex) {
			return $"Failed: {ex.Message}";
		}
	}

	/// <param name="date"> Date to check booking status</param>
	/// <returns> Check Booking status of the given date </returns>
	[KernelFunction, Description("Given a date, check its booking status")]
	public string CheckBookingStatus(
		[Description("The date to check its booking status]")] string Date) {

		DateTime date = DateTime.ParseExact(Date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
		if (date < DateTime.UtcNow.Date) {
			return $"Failed: Date must be a future date!";
		}

		if (!nftService.IsConnected) { return "Please connect your Wallet first!"; }

		string message = $"There is no booking on {date.ToString("dd/MM/yyyy")}. Please reconfirm if you want to make booking.";
		NFTItem query = nftService.Lookup(date).Result;
		if (query.Id != 0) {
			if (query.Owner == nftService.MyAddress) {
				message = $"You have already booked {date.ToString("dd/MM/yyyy")}. The booking Id is {query.Id}";
				if (query.Vacant) {
					message += $" Please reconfirm if you want to Reclaim booking Id {query.Id}.";
				} else {
					message += $" Please reconfirm if you want to release booking Id {query.Id}.";
				}	
			} else {
				message = $"Booking on {date.ToString("dd/MM/yyyy")} is currently owned by {query.Owner}.";
				if (query.Vacant) {
					message += " You can request the owner to tranfer it to you.";
				} else {
					message += " You cannot book, release or reclaim on this date.";
				}
			}
		}
		return message;
	}

	[KernelFunction, Description("List my bookings with booking status")]
	public string ListMyBookings() {
		if (!nftService.IsConnected) { return "Please connect your Wallet first!"; }

		string message = $"Your bookings with booking status are:";
		NFTItem[] items = nftService.ListNFT("My").Result;
		foreach (NFTItem item in items) {
			message += $"\n{item.BookingDate.ToString("dd/MM/yyyy")}, booking status:{(item.Vacant ? "Vacant" : "Occupied")}";
		}
		return message;
	}

	/// <param name="Id"> The Booking Id to release</param>
	/// <returns> Result of releasing, either Success or Failed. </returns>
	[KernelFunction, Description("Release a booking Id")]
	public string ReleaseBooking(
		[Description("The Booking Id to release")] int Id) {

		if (!nftService.IsConnected) { return "Please connect your Wallet first!"; }

		try {
			NFTItem result = nftService.Release(Id).Result;
			return $"Success: {result.BookingDate.ToString("dd/MM/yyyy")} Released.";
		} catch (Exception ex) {
			return $"Failed: {ex.Message}";
		}
	}

	/// <param name="start-date"> Start Date to list available dates</param>
	/// <param name="end-date"> End Date to list available dates</param>
	/// <returns>A list of available dates in a date range. </returns>
	[KernelFunction, Description("List of available dates in a date range")]
	public string ListAvailableDatesinDateRange(
		[Description("Start Date to list available dates")] string start_date,
		[Description("End Date to list available dates")] string end_date) {

		if (!nftService.IsConnected) { return "Please connect your Wallet first!"; }

		DateTime sdate = DateTime.ParseExact(start_date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
		DateTime edate = DateTime.ParseExact(end_date, "dd/MM/yyyy", CultureInfo.InvariantCulture);

		if (sdate > edate) {
			return $"Failed: Start Date must be earlier than End Date!";
		}
		if (sdate < DateTime.UtcNow.Date) {
			return $"Failed: Booking must be on a future date!";
		}

		NFTItem[] items = nftService.ListNFT("All").Result;

		string message = "";
		int count = 0;
		for (DateTime date = sdate; date <= edate; date = date.AddDays(1)) {
			NFTItem query = items.Where(x => x.BookingDate == date).FirstOrDefault();
			if (query == null) {
				message += date.Date.ToString("dd/MM/yyyy") + "\n";
				count++;
			} else {
				if (query.Owner == nftService.MyAddress) {
					message = $"{date.ToString("dd/MM/yyyy")}*";
					if (query.Vacant) {
						message += "*\n";
					} else {
						message += "\n";
					}
					count++;
				} else {
					if (query.Vacant) {
						message += $"{date.ToString("dd/MM/yyyy")}***\n";
						count++;
					}
				}
			}
			if (count > 10) {
				break;
			}
		}
		if (count <= 10) {
			message = "The following dates are available\n" + message;
		} else {
			message = "The date range is too large. Only the first 10 available dates are shown\n" + message;
		}
		message += "Remarks:\n";
		message += "* You have already booked this date\n";
		message += "** You have already booked this date but it is vacant. You should Reclaim it\n";
		message += "*** It is owned by other person but vacant. You can request the owner to transfer it to you\n";
		Debug.WriteLine($"*************AvailableDates: {message}");
		return message;
	}
}

