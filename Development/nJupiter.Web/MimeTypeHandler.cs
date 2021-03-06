#region Copyright & License
/*
	Copyright (c) 2005-2011 nJupiter

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Web;

namespace nJupiter.Web  {
	public class MimeTypeHandler : IMimeTypeHandler {

		private readonly HttpContextBase context;
		private readonly IHttpContextHandler contextHandler;

		private HttpContextBase CurrentContext { get { return context ?? contextHandler.Current; } }

		public MimeTypeHandler() : this(null, null) {}
		public MimeTypeHandler(HttpContextBase context) : this(null, context) {}
		public MimeTypeHandler(Func<HttpContextBase> httpContextFactoryMethod) : this(new HttpContextHandler(httpContextFactoryMethod)) {}
		public MimeTypeHandler(IHttpContextHandler contextHandler) : this(contextHandler, null) {}

		private MimeTypeHandler(IHttpContextHandler contextHandler, HttpContextBase context) {
			this.contextHandler = contextHandler ?? HttpContextHandler.Instance;
			this.context = context;
		}
	
		public IEnumerable<IMimeType> GetAcceptedTypes() {
			var acceptedTypes = new List<IMimeType>();
			if(CurrentContext != null && CurrentContext.Request.AcceptTypes != null) {
				foreach(var acceptedType in CurrentContext.Request.AcceptTypes) {
					acceptedTypes.Add(new MimeType(acceptedType));
				}
			}
			return acceptedTypes;
		}

		public IMimeType GetHighestQuality(IMimeType mimeType) {
			return GetHighestQuality(mimeType, false);
		}

		public IMimeType GetHighestQuality(IMimeType mimeType, bool exactType) {
			var acceptedTypes = GetAcceptedTypes();
			IMimeType result = null;
			foreach(var m in acceptedTypes) {
				if(((exactType && m.EqualsExactType(mimeType)) || m.EqualsType(mimeType)) && (result == null || result.Quality < m.Quality)) {
					result = m;
				}
			}
			return result;
		}

		public IMimeType GetMimeType(Stream stream) {
			if(stream == null) {
				throw new ArgumentNullException("stream");
			}

			var maxContent = (int)stream.Length;

			if(maxContent > 4096)
				maxContent = 4096;

			var buf = new byte[maxContent];
			stream.Read(buf, 0, maxContent);

			string mime;

			//note: the CLR frees the data automatically returned in ppwzMimeOut     
			var result = NativeMethods.FindMimeFromData(IntPtr.Zero, null, buf, maxContent, null, 0, out mime, 0);

			if(result != 0)
				Marshal.ThrowExceptionForHR(result);

			if(mime != null && mime.IndexOf("/", StringComparison.Ordinal) > 0)
				return new MimeType(mime);

			return null;
		}

		public IMimeType GetMimeType(FileInfo file) {
			if(file == null) {
				throw new ArgumentNullException("file");
			}
			if(!file.Exists) {
				throw new FileNotFoundException(string.Format("{0} not found", file));
			}

			using(var fs = file.OpenRead()) {
				return GetMimeType(fs);
			}
		}
		
		public IMimeType GetMimeType(byte[] bytes) {
			if(bytes == null) {
				throw new ArgumentNullException("bytes");
			}
			using(Stream s = new MemoryStream(bytes)) {
				return GetMimeType(s);
			}
		}

	}

	internal static class NativeMethods {
		[DllImport("urlmon.dll", EntryPoint = "FindMimeFromData", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
		internal static extern int FindMimeFromData(IntPtr pbc,
			[MarshalAs(UnmanagedType.LPWStr)] string pwzUrl,
			[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1, SizeParamIndex = 3)] byte[] pBuffer,
			int cbSize,
			[MarshalAs(UnmanagedType.LPWStr)] string pwzMimeProposed, int dwMimeFlags,
			[MarshalAs(UnmanagedType.LPWStr)] out string ppwzMimeOut,
			int dwReserved);

	}
}
