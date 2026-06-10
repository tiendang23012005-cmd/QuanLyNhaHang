-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Máy chủ: 127.0.0.1
-- Thời gian đã tạo: Th5 21, 2026 lúc 12:06 PM
-- Phiên bản máy phục vụ: 10.4.32-MariaDB
-- Phiên bản PHP: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Cơ sở dữ liệu: `quanly_nhahang`
--

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `ban`
--

CREATE TABLE `ban` (
  `id_ban` int(11) NOT NULL,
  `suc_chua` int(11) DEFAULT NULL,
  `trang_thai` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT 'Trống'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `ban`
--

INSERT INTO `ban` (`id_ban`, `suc_chua`, `trang_thai`) VALUES
(1, 4, 'Trống'),
(2, 6, 'Đang phục vụ');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `chitietorder`
--

CREATE TABLE `chitietorder` (
  `id_order` int(11) NOT NULL,
  `id_mon` int(11) NOT NULL,
  `so_luong` int(11) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `chitietorder`
--

INSERT INTO `chitietorder` (`id_order`, `id_mon`, `so_luong`) VALUES
(1, 1, 1),
(1, 2, 1),
(1, 4, 4),
(2, 1, 1),
(2, 3, 2),
(2, 4, 2);

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `doanhthu`
--

CREATE TABLE `doanhthu` (
  `id_doanh_thu` int(11) NOT NULL,
  `id_hoa_don` int(11) NOT NULL,
  `tong_tien` decimal(12,2) NOT NULL,
  `ngay_tinh` date DEFAULT curdate(),
  `ghi_chu` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `doanhthu`
--

INSERT INTO `doanhthu` (`id_doanh_thu`, `id_hoa_don`, `tong_tien`, `ngay_tinh`, `ghi_chu`) VALUES
(1, 1, 150000.00, '2025-11-13', 'Khách thanh toán tiền mặt'),
(2, 2, 170000.00, '2025-12-20', NULL),
(3, 3, 125000.00, '2025-12-20', NULL);

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `hethongsession`
--

CREATE TABLE `hethongsession` (
  `session_id` int(11) NOT NULL,
  `user_id` int(11) NOT NULL,
  `login_time` datetime DEFAULT current_timestamp(),
  `logout_time` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `hethongsession`
--

INSERT INTO `hethongsession` (`session_id`, `user_id`, `login_time`, `logout_time`) VALUES
(1, 3, '2025-12-20 19:16:10', NULL);

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `hoadon`
--

CREATE TABLE `hoadon` (
  `id_hoa_don` int(11) NOT NULL,
  `id_order` int(11) NOT NULL,
  `thoi_gian` datetime DEFAULT current_timestamp(),
  `trang_thai` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT 'Chưa thanh toán'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `hoadon`
--

INSERT INTO `hoadon` (`id_hoa_don`, `id_order`, `thoi_gian`, `trang_thai`) VALUES
(1, 1, '2025-11-13 19:11:42', 'Đã thanh toán'),
(2, 1, '2025-12-20 19:18:24', 'Đã thanh toán'),
(3, 2, '2025-12-20 19:21:16', 'Đã thanh toán');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `monan`
--

CREATE TABLE `monan` (
  `id_mon` int(11) NOT NULL,
  `id_nhom` int(11) DEFAULT NULL,
  `ten_mon` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `gia_tien` decimal(10,2) NOT NULL,
  `mo_ta` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `trang_thai` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT 'Còn món'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `monan`
--

INSERT INTO `monan` (`id_mon`, `id_nhom`, `ten_mon`, `gia_tien`, `mo_ta`, `trang_thai`) VALUES
(1, 1, 'Gỏi cuốn tôm thịt', 35000.00, 'Khai vị tươi mát', 'Còn món'),
(2, 2, 'Cơm chiên hải sản', 55000.00, 'Món chính được ưa thích', 'Còn món'),
(3, 3, 'Chè thập cẩm', 25000.00, 'Tráng miệng ngọt mát', 'Còn món'),
(4, 4, 'Cà phê sữa đá', 20000.00, 'Đồ uống quen thuộc', 'Còn món');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `nhommonan`
--

CREATE TABLE `nhommonan` (
  `id_nhom` int(11) NOT NULL,
  `ten_nhom` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `nhommonan`
--

INSERT INTO `nhommonan` (`id_nhom`, `ten_nhom`) VALUES
(1, 'Khai vị'),
(2, 'Món chính'),
(3, 'Tráng miệng'),
(4, 'Nước uống');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `order`
--

CREATE TABLE `order` (
  `id_order` int(11) NOT NULL,
  `id_ban` int(11) DEFAULT NULL,
  `id_nhan_vien` int(11) DEFAULT NULL,
  `thoi_gian` datetime DEFAULT current_timestamp(),
  `trang_thai` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT 'Đang xử lý'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `order`
--

INSERT INTO `order` (`id_order`, `id_ban`, `id_nhan_vien`, `thoi_gian`, `trang_thai`) VALUES
(1, 1, 2, '2025-11-13 19:11:42', 'Đã thanh toán'),
(2, 1, 3, '2025-12-20 19:18:41', 'Đã thanh toán');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `taikhoan`
--

CREATE TABLE `taikhoan` (
  `id_tai_khoan` int(11) NOT NULL,
  `tai_khoan` varchar(50) NOT NULL,
  `mat_khau` varchar(255) NOT NULL,
  `user_id` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `taikhoan`
--

INSERT INTO `taikhoan` (`id_tai_khoan`, `tai_khoan`, `mat_khau`, `user_id`) VALUES
(1, 'admin', '123456', 1),
(2, 'nhanvien', '123456', 2),
(3, 'DangVanTien', '$2y$10$NIb35O8EmgNDxXe5T.0sdONSnL6fwNmqHlN2yZEXdmgoUSkdefecK', 3);

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `user`
--

CREATE TABLE `user` (
  `user_id` int(11) NOT NULL,
  `ten` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `ten_quan` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `role` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `sdt` varchar(15) DEFAULT NULL,
  `email` varchar(100) DEFAULT NULL,
  `id_quan_ly` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `user`
--

INSERT INTO `user` (`user_id`, `ten`, `ten_quan`, `role`, `sdt`, `email`, `id_quan_ly`) VALUES
(1, 'Nguyen Van A', 'Quan A', 'Quản lý', '0901234567', 'a@example.com', NULL),
(2, 'Tran Thi B', 'Quan A', 'Nhân viên', '0907654321', 'b@example.com', NULL),
(3, 'Tien', 'Bánh kẹo', 'Quản lý', '0123456786', 'tiendang.23012005@gmail.com', NULL);

--
-- Chỉ mục cho các bảng đã đổ
--

--
-- Chỉ mục cho bảng `ban`
--
ALTER TABLE `ban`
  ADD PRIMARY KEY (`id_ban`);

--
-- Chỉ mục cho bảng `chitietorder`
--
ALTER TABLE `chitietorder`
  ADD PRIMARY KEY (`id_order`,`id_mon`),
  ADD KEY `id_mon` (`id_mon`);

--
-- Chỉ mục cho bảng `doanhthu`
--
ALTER TABLE `doanhthu`
  ADD PRIMARY KEY (`id_doanh_thu`),
  ADD KEY `id_hoa_don` (`id_hoa_don`);

--
-- Chỉ mục cho bảng `hethongsession`
--
ALTER TABLE `hethongsession`
  ADD PRIMARY KEY (`session_id`),
  ADD KEY `user_id` (`user_id`);

--
-- Chỉ mục cho bảng `hoadon`
--
ALTER TABLE `hoadon`
  ADD PRIMARY KEY (`id_hoa_don`),
  ADD KEY `id_order` (`id_order`);

--
-- Chỉ mục cho bảng `monan`
--
ALTER TABLE `monan`
  ADD PRIMARY KEY (`id_mon`),
  ADD KEY `id_nhom` (`id_nhom`);

--
-- Chỉ mục cho bảng `nhommonan`
--
ALTER TABLE `nhommonan`
  ADD PRIMARY KEY (`id_nhom`);

--
-- Chỉ mục cho bảng `order`
--
ALTER TABLE `order`
  ADD PRIMARY KEY (`id_order`),
  ADD KEY `id_ban` (`id_ban`),
  ADD KEY `id_nhan_vien` (`id_nhan_vien`);

--
-- Chỉ mục cho bảng `taikhoan`
--
ALTER TABLE `taikhoan`
  ADD PRIMARY KEY (`id_tai_khoan`),
  ADD UNIQUE KEY `tai_khoan` (`tai_khoan`),
  ADD KEY `user_id` (`user_id`);

--
-- Chỉ mục cho bảng `user`
--
ALTER TABLE `user`
  ADD PRIMARY KEY (`user_id`),
  ADD KEY `id_quan_ly` (`id_quan_ly`);

--
-- AUTO_INCREMENT cho các bảng đã đổ
--

--
-- AUTO_INCREMENT cho bảng `ban`
--
ALTER TABLE `ban`
  MODIFY `id_ban` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT cho bảng `doanhthu`
--
ALTER TABLE `doanhthu`
  MODIFY `id_doanh_thu` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT cho bảng `hethongsession`
--
ALTER TABLE `hethongsession`
  MODIFY `session_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT cho bảng `hoadon`
--
ALTER TABLE `hoadon`
  MODIFY `id_hoa_don` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT cho bảng `monan`
--
ALTER TABLE `monan`
  MODIFY `id_mon` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT cho bảng `nhommonan`
--
ALTER TABLE `nhommonan`
  MODIFY `id_nhom` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT cho bảng `order`
--
ALTER TABLE `order`
  MODIFY `id_order` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT cho bảng `taikhoan`
--
ALTER TABLE `taikhoan`
  MODIFY `id_tai_khoan` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT cho bảng `user`
--
ALTER TABLE `user`
  MODIFY `user_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- Các ràng buộc cho các bảng đã đổ
--

--
-- Các ràng buộc cho bảng `chitietorder`
--
ALTER TABLE `chitietorder`
  ADD CONSTRAINT `chitietorder_ibfk_1` FOREIGN KEY (`id_order`) REFERENCES `order` (`id_order`) ON DELETE CASCADE,
  ADD CONSTRAINT `chitietorder_ibfk_2` FOREIGN KEY (`id_mon`) REFERENCES `monan` (`id_mon`);

--
-- Các ràng buộc cho bảng `doanhthu`
--
ALTER TABLE `doanhthu`
  ADD CONSTRAINT `doanhthu_ibfk_1` FOREIGN KEY (`id_hoa_don`) REFERENCES `hoadon` (`id_hoa_don`);

--
-- Các ràng buộc cho bảng `hethongsession`
--
ALTER TABLE `hethongsession`
  ADD CONSTRAINT `hethongsession_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `user` (`user_id`);

--
-- Các ràng buộc cho bảng `hoadon`
--
ALTER TABLE `hoadon`
  ADD CONSTRAINT `hoadon_ibfk_1` FOREIGN KEY (`id_order`) REFERENCES `order` (`id_order`);

--
-- Các ràng buộc cho bảng `monan`
--
ALTER TABLE `monan`
  ADD CONSTRAINT `monan_ibfk_1` FOREIGN KEY (`id_nhom`) REFERENCES `nhommonan` (`id_nhom`);

--
-- Các ràng buộc cho bảng `order`
--
ALTER TABLE `order`
  ADD CONSTRAINT `order_ibfk_1` FOREIGN KEY (`id_ban`) REFERENCES `ban` (`id_ban`),
  ADD CONSTRAINT `order_ibfk_2` FOREIGN KEY (`id_nhan_vien`) REFERENCES `user` (`user_id`);

--
-- Các ràng buộc cho bảng `taikhoan`
--
ALTER TABLE `taikhoan`
  ADD CONSTRAINT `taikhoan_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `user` (`user_id`);

--
-- Các ràng buộc cho bảng `user`
--
ALTER TABLE `user`
  ADD CONSTRAINT `user_ibfk_1` FOREIGN KEY (`id_quan_ly`) REFERENCES `user` (`user_id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
