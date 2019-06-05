using System;
using System.IO;

namespace Veldrid.SampleGallery
{
    public class ReloadablePipeline : IDisposable
    {
        private readonly ResourceFactory _factory;
        private GraphicsPipelineDescription _pipelineDesc;
        private bool _isDisposed = false;

        private readonly string _vertexPath;
        private FileSystemWatcher _vertexWatcher;
        private bool _vertexChanged;
        private Shader _vertexShader;

        private readonly string _fragmentPath;
        private readonly FileSystemWatcher _fragmentWatcher;
        private bool _fragmentChanged;
        private Shader _fragmentShader;

        private Pipeline _pipeline;

        public ReloadablePipeline(
            ResourceFactory factory,
            string vertexPath,
            string fragmentPath,
            GraphicsPipelineDescription pipelineDesc)
        {
            _factory = factory;

            _vertexPath = vertexPath;
            _vertexWatcher = new FileSystemWatcher(_vertexPath);
            _vertexWatcher.Changed += (s, e) => _vertexChanged = true;

            _fragmentPath = fragmentPath;
            _fragmentWatcher = new FileSystemWatcher(_fragmentPath);
            _fragmentWatcher.Changed += (s, e) => _fragmentChanged = true;

            _pipelineDesc = pipelineDesc;
        }

        public Pipeline GetPipeline()
        {
            bool eitherChanged = _vertexChanged || _fragmentChanged;

            if (_vertexChanged)
            {
                _vertexChanged = false;
                try
                {
                    Shader newVertexShader = _factory.CreateShader(
                        new ShaderDescription(ShaderStages.Vertex, File.ReadAllBytes(_vertexPath), "main"));
                    _vertexShader?.Dispose();
                    _vertexShader = newVertexShader;
                }
                catch { }
            }
            if (_fragmentChanged)
            {
                _fragmentChanged = false;
                try
                {
                    Shader newFragmentShader = _factory.CreateShader(
                        new ShaderDescription(ShaderStages.Fragment, File.ReadAllBytes(_fragmentPath), "main"));
                    _fragmentShader?.Dispose();
                    _fragmentShader = newFragmentShader;
                }
                catch { }
            }

            if (eitherChanged)
            {
                _pipelineDesc.ShaderSet.Shaders = new[] { _vertexShader, _fragmentShader };
                _pipeline?.Dispose();
                _pipeline = _factory.CreateGraphicsPipeline(_pipelineDesc);
            }

            return _pipeline;
        }

        ~ReloadablePipeline()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _isDisposed = true;

                if (disposing)
                {
                    _vertexWatcher.Dispose();
                    _fragmentWatcher.Dispose();
                }

                _vertexShader?.Dispose();
                _fragmentShader?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
