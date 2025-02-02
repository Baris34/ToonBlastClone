public enum BoardState
{
    Idle,       // Kullan�c� t�klamas�n� bekliyor
    Removing,   // Blok patlatma animasyonu
    Gravity,    // Bloklar�n d�����
    Refill,     // Eksik h�creleri doldurma
    Checking,   // BFS ile no moves? => Shuffle?
    Shuffling,  // Shuffle animasyonu
    Busy        // Ge�ici durum (opsiyonel)
}