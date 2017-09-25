using System;

namespace InSearch.Core.Data
{
    public class _DbContextScope : IDisposable
    {
        private readonly bool _autoDetectChangesEnabled;
        private readonly bool _proxyCreationEnabled;
        private readonly bool _validateOnSaveEnabled;
        private readonly IDbContext _ctx;

        public _DbContextScope(IDbContext ctx = null, bool? autoDetectChanges = null, bool? proxyCreation = null, bool? validateOnSave = null)
        {
            _ctx = ctx ?? EngineContext.Current.Resolve<IDbContext>();
            _autoDetectChangesEnabled = _ctx.AutoDetectChangesEnabled;
            _proxyCreationEnabled = _ctx.ProxyCreationEnabled;
            _validateOnSaveEnabled = _ctx.ValidateOnSaveEnabled;

            if (autoDetectChanges.HasValue)
                _ctx.AutoDetectChangesEnabled = autoDetectChanges.Value;

            if (proxyCreation.HasValue)
                _ctx.ProxyCreationEnabled = proxyCreation.Value;

            if (validateOnSave.HasValue)
                _ctx.ValidateOnSaveEnabled = validateOnSave.Value;
        }

        public int Commit()
        {
            return _ctx.SaveChanges();
        }

        public void Dispose()
        {
            _ctx.AutoDetectChangesEnabled = _autoDetectChangesEnabled;
            _ctx.ProxyCreationEnabled = _proxyCreationEnabled;
            _ctx.ValidateOnSaveEnabled = _validateOnSaveEnabled;
        }
    }
}
