using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PDWSIM
{
	public class PDWSimulator
	{
		public static void Main(string[] args)
		{
			
			int i = 0;

			UdpClient client = new UdpClient();
			IPEndPoint ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 15000); //send datagram to localhost on port 15000

			PDW pdw = new PDW();
			var d = pdw.Serialize();

			// Set duration of PDW playback here
			int sec = 5;  //seconds

			// Start timer
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			for (i = 0; i <= 1000000000; i++)
			{
				// check stop time every 1000 iterations
				if (i % 1000 == 0)
				{
					//pdw.startFrequencyMHz = 4500; 

					if (stopwatch.Elapsed > TimeSpan.FromSeconds(sec))
						break;
				}
				
				Task.Run(() => 
				{
					// modifying values directly in byte array to avoid serialization each loop iteration - for speed of simulator only.  Don't do this in real life! 
					// Every 150th Freq = 4500.5
					if (i % 150 == 0)
					{
						// 4500.5
						d[19] = 0x45; d[18] = 0x8c; d[17] = 0xa4; d[16] = 0x00;
					}
					// Every 1501th Freq is 5442
					else if (i % 15001 == 0)
					{
						//5442
						d[19] = 0x45; d[18] = 0xaa; d[17] = 0x10; d[16] = 0x00;
					}
					// Set all other Freqs = 0
					else
					{
						d[19] = 0; d[18] = 0; d[17] = 0; d[16] = 0;
					}

					// Send datagram
					client.SendAsync(d, d.Length, ip);
				});

			}

			client.Dispose();
			Console.WriteLine("Total datagrams = " + i);
			Console.WriteLine("Total datagrams per sec = " + (i / sec));

		}
	}

	public class PDW
	{
		public int counter = 0; // rolling counter 
		public int receiverID = 1; //!< ID of the receiver that generated the PDW
		
		public long toa;

		public float startFrequencyMHz = 1000; //!< Start frequency in MHz (TOA)
		public float stopFrequencyMHz = 1000; //!< Stop frequency in MHz (TOA + PW)
		public double pulseWidth = 0.0005; //!< Pulse width in seconds
		public float pulseAmplitude = -5.4F; //! Pulse amplitude in dBm (verify not dBW)
		public float snr = 10; //!< Signal to noise ratio in dB
		public float phase = 0; //!< Phase in radians
		public float phaseReferenceTime = 0; //!< Offset from TOA of phase measurement in seconds
		public int errorFlags = 0;  // placeholder for error flags
		public int modulationFlags = 0;
		public float modulationParameters = 0; //!< in Hz or Hz/s as defined by EMD platinum
		public float angleOfArrival = 0; //!< in radians
		public float riseTime = 0; //!< Rise time in seconds
		public float fallTime = 0; //!< Rise time in seconds
		public double pri = 0; //!< Pulse repetition interval in seconds
	


	public byte[] Serialize()
	{
		using (MemoryStream m = new MemoryStream())
		{
			using (BinaryWriter writer = new BinaryWriter(m))
			{
				writer.Write(counter);
				writer.Write(receiverID);
				writer.Write(toa);
				writer.Write(startFrequencyMHz);
				writer.Write(stopFrequencyMHz);
				writer.Write(pulseWidth);
				writer.Write(pulseAmplitude);
				writer.Write(snr);
				writer.Write(phase);
				writer.Write(phaseReferenceTime);
				writer.Write(errorFlags);
				writer.Write(modulationFlags);
				writer.Write(modulationParameters);
				writer.Write(angleOfArrival);
				writer.Write(riseTime);
				writer.Write(fallTime);
				writer.Write(pri);

			}
			return m.ToArray();
		}
	}

}

}
