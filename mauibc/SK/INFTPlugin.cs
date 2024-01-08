using Azure.AI.OpenAI;
using System.Text.Json;
using System;
using System.ComponentModel;
using Microsoft.SemanticKernel;
using System.Diagnostics;
using MAUIBC;

/// <summary>
/// NFTService provides a set of Bookings entry and booking enquiry functions.
/// </summary>
namespace ChatMAUI;
public interface INFTPlugin {
	//public INFTService nftService { get; set; }
	public string MakeBooking(string Date);
	public string ReclaimBooking(int Id);
	public string CheckBookingStatus(string Date);
	public string ReleaseBooking(int Id);
	public string ListAvailableDatesinDateRange(string start_date, string end_date);
	public string ListMyBookings();
}

