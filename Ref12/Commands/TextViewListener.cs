﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using SLaks.Ref12.Services;

namespace SLaks.Ref12.Commands {
	[Export(typeof(IWpfTextViewConnectionListener))]
	[ContentType("CSharp")]
	[ContentType("Basic")]
	[TextViewRole(PredefinedTextViewRoles.Document)]
	public class TextViewListener : IWpfTextViewConnectionListener {
		[Import]
		public SVsServiceProvider ServiceProvider { get; set; }

		[ImportMany]
		public IEnumerable<IReferenceSourceProvider> ReferenceProviders { get; set; }

		[Import]
		public IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; set; }
		[Import]
		public ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

		public void SubjectBuffersConnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers) {
			if (RoslynUtilities.IsRoslynInstalled(ServiceProvider))
				return;

			if (!subjectBuffers.Any(b => b.ContentType.IsOfType("CSharp") || b.ContentType.IsOfType("Basic")))
				return;

			var textViewAdapter = EditorAdaptersFactoryService.GetViewAdapter(textView);
			if (textViewAdapter == null)
				return;
			ITextDocument document;
			if (!TextDocumentFactoryService.TryGetTextDocument(textView.TextDataModel.DocumentBuffer, out document))
				return;

			textView.Properties.GetOrCreateSingletonProperty(() => new GoToDefinitionInterceptor(ReferenceProviders, textViewAdapter, textView, document));
		}
		public void SubjectBuffersDisconnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers) {
		}
	}
}