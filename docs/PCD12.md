# **Laporan Teknis Lengkap: Image Enhancement & Panduan Implementasi Tugas Terperinci**

Laporan ini merupakan panduan komprehensif yang merangkum materi perkuliahan mengenai perbaikan kualitas citra (*Image Enhancement*) dan rincian spesifikasi teknis untuk tugas praktikum pemrosesan citra digital.

## **1\. Landasan Teori Mendalam: Image Enhancement**

*Image Enhancement* bukan sekadar estetika, melainkan proses teknis untuk meningkatkan kemudahan interpretasi informasi dalam citra. Tujuan utamanya adalah untuk memproses citra agar hasilnya "lebih baik" daripada citra asli untuk aplikasi tertentu, seperti diagnosis medis, penginderaan jauh, atau pengenalan pola otomatis.

### **A. Ranah Pemrosesan (Processing Domains)**

Pemrosesan citra dikategorikan berdasarkan cara data diakses dan dimanipulasi:

1. **Ranah Spasial (Spatial Domain):** Manipulasi dilakukan langsung pada nilai intensitas pixel di posisi $(x, y)$. Ini adalah pendekatan yang paling intuitif. Secara matematis dinyatakan sebagai:$$g(x, y) \= T\[f(x, y)\]$$  
   Di mana $f(x, y)$ adalah citra masukan, $g(x, y)$ adalah citra keluaran, dan $T$ adalah operator transformasi yang bekerja pada lingkungan sekitar $(x, y)$.  
2. **Ranah Frekuensi (Frequency Domain):** Citra diubah menjadi komponen frekuensi (sinus dan kosinus) menggunakan **Fourier Transform**. Pendekatan ini sangat efektif untuk menghilangkan gangguan periodik atau melakukan operasi pemfilteran kompleks yang sulit dilakukan di ranah spasial. Setelah pemrosesan, citra dikembalikan ke domain spasial menggunakan *Inverse Fourier Transform*.

### **B. Klasifikasi Hierarki Operasi Spasial**

* **Aras Titik (Point Processing):** Operasi yang hanya bergantung pada nilai intensitas pixel individual tanpa mempedulikan tetangganya. Contoh umum adalah pengaturan kecerahan (*brightness*), kontras, dan pembalikan citra (*negative*).  
* **Aras Lokal (Neighborhood Processing):** Operasi yang menghitung nilai baru pixel berdasarkan nilai pixel itu sendiri dan pixel di sekitarnya. Teknik ini menggunakan **Konvolusi** dengan jendela matriks kecil yang disebut *Kernel* atau *Mask*. Digunakan untuk penghalusan (*blurring*) dan penajaman (*sharpening*).  
* **Aras Global:** Operasi yang mempertimbangkan distribusi seluruh nilai pixel dalam citra (statistik citra). Contoh paling populer adalah *Histogram Equalization* yang mendistribusikan ulang intensitas untuk meningkatkan kontras secara keseluruhan.

## **2\. Detil Teknis Tugas (Requirement Spesifikasi)**

Mahasiswa diwajibkan untuk mengimplementasikan modul-modul berikut dengan memperhatikan detail algoritma di bawah ini:

### **Tugas I: Operasi Kontras (Contrast Enhancement)**

Tugas ini bertujuan memperbaiki citra yang memiliki distribusi intensitas yang sempit (terlalu gelap atau terlalu pudar). Implementasikan pada tiga aras:

1. **Aras Titik (Point Operation):**  
   * Gunakan transformasi linier: $s \= a \\cdot r \+ b$.  
   * **Implikasi:** Jika $a \> 1$, kontras meningkat; jika $a \< 1$, kontras menurun. Jika $b \> 0$, kecerahan meningkat.  
   * *Tambahan:* Implementasikan **Gamma Correction** ($s \= c \\cdot r^{\\gamma}$) untuk melihat perbaikan non-linier pada area bayangan.  
2. **Aras Lokal (Local Operation):**  
   * Implementasikan *Adaptive Contrast Enhancement*.  
   * Hitung rata-rata lokal ($\\mu\_{local}$) dan standar deviasi lokal ($\\sigma\_{local}$) pada jendela $3 \\times 3$. Gunakan parameter penguatan untuk menyesuaikan pixel pusat relatif terhadap kontras di sekitarnya.  
3. **Aras Global (Global Operation):**  
   * Implementasikan **Linear Contrast Stretching (Normalization)**.  
   * Rumus:$$P\_{out} \= (P\_{in} \- P\_{min}) \\times \\frac{L-1}{P\_{max} \- P\_{min}}$$  
   * Di mana $P\_{min}$ dan $P\_{max}$ adalah nilai intensitas terkecil dan terbesar yang sebenarnya ada di dalam citra. Operasi ini akan "meregangkan" histogram hingga mengisi penuh rentang $\[0, 255\]$.

### **Tugas II: Pelembutan Citra (Image Smoothing)**

Pelembutan digunakan untuk mengurangi *noise* atau mengaburkan detail yang tidak relevan. Implementasikan 3 jenis kernel:

1. **Gaussian Blur (Wajib):**  
   * Kernel ini mengikuti distribusi lonceng (Gaussian) yang memberikan bobot tertinggi pada pixel pusat dan menurun secara halus ke arah tepi.  
   * Contoh Kernel $3 \\times 3$ (Normalisasi $\\frac{1}{16}$):$$\\frac{1}{16} \\begin{bmatrix} 1 & 2 & 1 \\\\ 2 & 4 & 2 \\\\ 1 & 2 & 1 \\end{bmatrix}$$  
2. **Mean Filter (Box Blur):**  
   * Menghitung rata-rata aritmatika dari semua pixel dalam jendela. Sangat efektif untuk menghilangkan *grainy noise*.  
3. **Median Filter (Pencarian Mandiri):**  
   * Mengurutkan nilai pixel dalam jendela dan mengambil nilai tengahnya. Ini adalah teknik non-linier yang sangat ampuh untuk menghilangkan *Salt and Pepper noise* tanpa mengaburkan tepi objek setajam Mean Filter.

### **Tugas III: Penajaman Citra (Image Sharpening)**

Penajaman bertujuan untuk memperjelas batas antar objek.

* Gunakan kernel **Laplacian**. Kernel ini bekerja dengan menghitung turunan kedua dari intensitas citra.  
* Contoh Kernel:$$\\begin{bmatrix} 0 & \-1 & 0 \\\\ \-1 & 4 & \-1 \\\\ 0 & \-1 & 0 \\end{bmatrix} \\quad \\text{atau} \\quad \\begin{bmatrix} \-1 & \-1 & \-1 \\\\ \-1 & 8 & \-1 \\\\ \-1 & \-1 & \-1 \\end{bmatrix}$$  
* *Catatan:* Hasil Laplacian biasanya ditambahkan kembali ke citra asli untuk mendapatkan citra yang tajam.

### **Tugas IV: Pewarnaan Semu & Seleksi Warna Interaktif**

Tugas ini menguji kemampuan manipulasi pixel secara kondisional berdasarkan input user.

* Logika Interaksi: 1\. User melakukan MouseDown $\\rightarrow$ Program menangkap koordinat $(x, y)$.  
  2\. Ambil nilai intensitas pixel tersebut sebagai referensi ($R\_{ref}, G\_{ref}, B\_{ref}$).  
* **Perhitungan Toleransi:**  
  * Gunakan jarak Euclidean warna atau selisih nilai *grayscale*.  
  * Jika jarak antar pixel $(R\_i, G\_i, B\_i)$ dengan referensi $\\leq$ Ambang Batas (Toleransi), maka pixel tetap berwarna asli.  
  * Jika di luar ambang batas, ubah pixel menjadi skala abu-abu atau turunkan kecerahannya secara signifikan (efek *spotlight*).  
* **Slider Toleransi:** Input $0\\% \- 100\\%$ harus mengubah sensitivitas deteksi kemiripan warna secara *real-time*.

### **Tugas V: Keamanan Citra (Fitur Pengayaan)**

1. **Steganografi (LSB):** Sembunyikan pesan dalam bit paling tidak signifikan (*Least Significant Bit*) dari setiap byte warna. Pastikan pesan dapat diekstrak kembali dengan sempurna.  
2. **Digital Watermarking:** Sisipkan logo transparan atau teks penanda ke dalam citra. Jelaskan bagaimana *watermark* tersebut bereaksi jika citra dimanipulasi (misalnya dipotong atau diubah kontrasnya).

## **3\. Ketentuan Pengumpulan & Standar Kualitas**

1. **Efisiensi Kode:** Hindari penggunaan *nested loop* triple yang tidak perlu pada citra besar. Gunakan teknik pemrosesan array atau library yang dioptimasi jika memungkinkan.  
2. **Komentar Kode (Mandatori):** Setiap fungsi pemrosesan pixel harus memiliki komentar yang menjelaskan parameter masukan, keluaran, dan algoritma yang digunakan.  
3. **Analisis Video:**  
   * Demonstrasikan perbedaan visual yang nyata.  
   * Berikan penjelasan mengapa suatu metode (misal Median vs Mean) memberikan hasil yang berbeda pada citra yang sama.  
4. **Validasi Histogram:** Laporan harus menyertakan visualisasi histogram. Perubahan pada kontras harus terlihat jelas sebagai pelebaran atau pergeseran distribusi pada grafik histogram tersebut.

*Laporan ini dirancang untuk memastikan mahasiswa memahami aspek teoritis dan praktis dari manipulasi data citra digital secara mendalam.*