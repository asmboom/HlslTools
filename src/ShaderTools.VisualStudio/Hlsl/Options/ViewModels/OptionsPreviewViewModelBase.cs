﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Formatting;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.VisualStudio.Core.Util;
using ShaderTools.VisualStudio.Hlsl.Util.Extensions;

namespace ShaderTools.VisualStudio.Hlsl.Options.ViewModels
{
    internal abstract class OptionsPreviewViewModelBase : NotifyPropertyChangedBase, IDisposable
    {
        private IWpfTextViewHost _textViewHost;

        private readonly IContentType _contentType;
        private readonly IEditorOptionsFactoryService _editorOptions;
        private readonly ITextEditorFactoryService _textEditorFactoryService;
        private readonly ITextBufferFactoryService _textBufferFactoryService;
        private readonly IProjectionBufferFactoryService _projectionBufferFactory;
        private readonly IContentTypeRegistryService _contentTypeRegistryService;
        private readonly IOptionsService _optionsService;

        public List<object> Items { get; }

        protected OptionsPreviewViewModelBase(IServiceProvider serviceProvider)
        {
            Items = new List<object>();

            var componentModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));

            _contentTypeRegistryService = componentModel.GetService<IContentTypeRegistryService>();
            _textBufferFactoryService = componentModel.GetService<ITextBufferFactoryService>();
            _textEditorFactoryService = componentModel.GetService<ITextEditorFactoryService>();
            _projectionBufferFactory = componentModel.GetService<IProjectionBufferFactoryService>();
            _editorOptions = componentModel.GetService<IEditorOptionsFactoryService>();
            _optionsService = componentModel.GetService<IOptionsService>();

            _contentType = _contentTypeRegistryService.GetContentType(HlslConstants.ContentTypeName);
        }

        public void SetOptionAndUpdatePreview<T>(T value, Option<T> option, string preview)
        {
            option.Value = value;
            UpdateDocument(preview);
        }

        public IWpfTextViewHost TextViewHost
        {
            get { return _textViewHost; }
            private set
            {
                _textViewHost?.Close();
                SetProperty(ref _textViewHost, value);
            }
        }

        public void UpdatePreview(string text)
        {
            const string start = "//[";
            const string end = "//]";

            var sourceText = SourceText.From(text);
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(sourceText);
            var edits = Formatter.GetEdits(syntaxTree, new TextSpan(sourceText, 0, text.Length), _optionsService.FormattingOptions);
            var formatted = Formatter.ApplyEdits(text, edits);

            var textBuffer = _textBufferFactoryService.CreateTextBuffer(formatted, _contentType);

            var bufferText = textBuffer.CurrentSnapshot.GetText().ToString();
            var startIndex = bufferText.IndexOf(start, StringComparison.Ordinal);
            var endIndex = bufferText.IndexOf(end, StringComparison.Ordinal);
            var startLine = textBuffer.CurrentSnapshot.GetLineNumberFromPosition(startIndex) + 1;
            var endLine = textBuffer.CurrentSnapshot.GetLineNumberFromPosition(endIndex);

            var projection = _projectionBufferFactory.CreateProjectionBufferWithoutIndentation(_contentTypeRegistryService,
                _editorOptions.CreateOptions(),
                textBuffer.CurrentSnapshot,
                "",
                LineSpan.FromBounds(startLine, endLine));

            var textView = _textEditorFactoryService.CreateTextView(projection,
              _textEditorFactoryService.CreateTextViewRoleSet(PredefinedTextViewRoles.Analyzable));

            this.TextViewHost = _textEditorFactoryService.CreateTextViewHost(textView, setFocus: false);
        }

        public void Dispose()
        {
            if (_textViewHost != null)
            {
                _textViewHost.Close();
                _textViewHost = null;
            }
        }

        private void UpdateDocument(string text)
        {
            UpdatePreview(text);
        }
    }
}