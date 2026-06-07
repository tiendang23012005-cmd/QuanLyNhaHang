// src/app/core/models/menu.model.ts

export interface MonAn {
  maMonAn: number;
  tenMonAn: string;
  gia: number;
  hinhAnh: string | null;
  moTa: string | null;
  conBan: boolean;
}

export interface CartItem {
  monAn: MonAn;
  soLuong: number;
}

// Model gửi lên Backend khi đặt hàng
export interface OrderRequest {
  loaiDonHang: string;
  phuongThucThanhToan: string;
  maBan?: number | null;  // Thêm
  ghiChu?: string;        // Thêm
  chiTietDonHang: any[];
}