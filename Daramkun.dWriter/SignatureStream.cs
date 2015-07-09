using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Daramkun.dWriter
{
	public class SignatureStream : Stream
	{
		Stream baseStream;

		public string Signature { get; private set; }

		public override bool CanRead { get { return baseStream.CanRead; } }
		public override bool CanSeek { get { return baseStream.CanSeek; } }
		public override bool CanWrite { get { return baseStream.CanWrite; } }

		public override void Flush () { baseStream.Flush (); }

		public override long Length { get { return baseStream.Length - Signature.Length; } }

		public override long Position
		{
			get { return baseStream.Position - Signature.Length; }
			set { baseStream.Position = value + Signature.Length; }
		}

		public SignatureStream ( string signature, Stream baseStream )
		{
			Signature = signature;
			this.baseStream = baseStream;
			if ( CanRead )
			{
				byte [] sig = new byte [ signature.Length ];
				try { baseStream.Read ( sig, 0, sig.Length ); }
				catch { throw new ArgumentException (); }
				if ( Encoding.UTF8.GetString ( sig, 0, sig.Length ) != Signature )
					throw new ArgumentException ();
			}
			else if ( CanWrite && CanSeek )
			{
				long pos = baseStream.Position;
				baseStream.Position = 0;
				byte [] sig = Encoding.UTF8.GetBytes ( signature );
				baseStream.Write ( sig, 0, sig.Length );
				baseStream.Position = pos + signature.Length;
			}
			else if ( CanWrite && !CanSeek && baseStream.Position == 0 )
			{
				byte [] sig = Encoding.UTF8.GetBytes ( signature );
				baseStream.Write ( sig, 0, sig.Length );	
			}
		}

		public override long Seek ( long offset, SeekOrigin origin )
		{
			return baseStream.Seek ( offset + ( ( origin == SeekOrigin.Begin ) ? Signature.Length : 0 ),
				origin ) - Signature.Length;
		}
		public override void SetLength ( long value ) { baseStream.SetLength ( value + Signature.Length ); }

		public override int Read ( byte [] buffer, int offset, int count ) { return baseStream.Read ( buffer, offset, count ); }
		public override void Write ( byte [] buffer, int offset, int count ) { baseStream.Write ( buffer, offset, count ); }
	}
}
