# ⚡ TaskFlow — Kurumsal Görev Yönetim Sistemi

WPF ve .NET 10 ile geliştirilmiş, kurumsal düzeyde 
bir görev ve proje yönetim masaüstü uygulaması.

## 🖥️ Teknolojiler

- **Platform:** WPF (.NET 10)
- **Mimari:** MVVM
- **Veritabanı:** SQLite + Entity Framework Core
- **UI Toolkit:** CommunityToolkit.Mvvm

## 👥 Kullanıcı Hiyerarşisi

| Rol | Yetkiler |
|-----|----------|
| Admin | Tüm sistem yönetimi |
| Departman Yöneticisi | Departman projeleri ve takımları |
| Takım Kaptanı | Takım projeleri ve görevleri |
| Çalışan | Atandığı projeler |

## 🚀 Özellikler

- Kullanıcı girişi ve rol tabanlı yetkilendirme
- Proje ve görev yönetimi
- Kanban ve Liste görünümü
- Commit sistemi ile ilerleme takibi
- Departman ve takım yönetimi
- Dashboard ile istatistikler

## ⚙️ Kurulum

1. Repoyu klonla: git clone https://github.com/kullaniciadi/TaskFlow.git
2. Visual Studio 2022 ile aç
3. NuGet paketlerini yükle
4. F5 ile çalıştır

## 🔑 Varsayılan Giriş
Kullanıcı adı : admin
Şifre         : admin123
