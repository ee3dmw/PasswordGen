namespace PasswordGen
{
    public interface IRepository<T>
    {
        void Save(T item);
        T Load(string id);
    }
}