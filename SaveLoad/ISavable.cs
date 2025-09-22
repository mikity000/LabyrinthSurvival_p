public interface ISavable {
    object SaveState();
    void LoadState(object state);
}