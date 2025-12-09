<template>
  <!-- Nhúng toàn bộ sprite SVG (ẩn) -->
  <div v-html="qltsIconSvg" style="position: absolute; width: 0; height: 0; overflow: hidden; opacity: 0; pointer-events: none;"></div>

  <!-- Khai báo tất cả symbol icons -->
  <svg style="display: none;" aria-hidden="true" focusable="false">
    <defs>
      <!-- Sidebar Icons -->
      <symbol id="icon-tong-quan" viewBox="0 0 24 24">
        <use href="#TrangChu" transform="translate(-20, -152)" />
      </symbol>
      <symbol id="icon-tong-quan-active" viewBox="0 0 24 24">
        <use href="#TrangChu-On" transform="translate(-20, -196)" />
      </symbol>
      <symbol id="icon-tai-san" viewBox="0 0 24 24">
        <use href="#TaiSan" transform="translate(-64, -152)" />
      </symbol>
      <symbol id="icon-tai-san-active" viewBox="0 0 24 24">
        <use href="#TaiSan-On" transform="translate(-64, -196)" />
      </symbol>
      <symbol id="icon-htdb" viewBox="0 0 24 24">
        <use href="#HTDB" transform="translate(-108, -152)" />
      </symbol>
      <symbol id="icon-htdb-active" viewBox="0 0 24 24">
        <use href="#HTDB-On" transform="translate(-108, -196)" />
      </symbol>
      <symbol id="icon-ccdc" viewBox="0 0 24 24">
        <use href="#CCDC" transform="translate(-152, -152)" />
      </symbol>
      <symbol id="icon-ccdc-active" viewBox="0 0 24 24">
        <use href="#On-2" transform="translate(-152, -196)" />
      </symbol>
      <symbol id="icon-danh-muc" viewBox="0 0 24 24">
        <use href="#DanhMuc" transform="translate(-196, -152)" />
      </symbol>
      <symbol id="icon-danh-muc-active" viewBox="0 0 24 24">
        <use href="#DanhMuc-On" transform="translate(-196, -196)" />
      </symbol>
      <symbol id="icon-tra-cuu" viewBox="0 0 24 24">
        <use href="#TraCuu" transform="translate(-240, -152)" />
      </symbol>
      <symbol id="icon-tra-cuu-active" viewBox="0 0 24 24">
        <use href="#TraCuu-On" transform="translate(-240, -196)" />
      </symbol>
      <symbol id="icon-bao-cao" viewBox="0 0 24 24">
        <use href="#BaoCao" transform="translate(-328, -152)" />
      </symbol>
      <symbol id="icon-bao-cao-active" viewBox="0 0 24 24">
        <use href="#BaoCao-On" transform="translate(-328, -196)" />
      </symbol>

      <!-- Header Icons -->
      <symbol id="icon-phan-hoi" viewBox="0 0 24 24">
        <use href="#PhanHoi" transform="translate(-460, -65)" />
      </symbol>
      <symbol id="icon-he-thong" viewBox="0 0 24 24">
        <use href="#HeThong" transform="translate(-195, -20)" />
      </symbol>
      <symbol id="icon-ho-tro" viewBox="0 0 24 24">
        <use href="#HoTRo" transform="translate(-63.5, -20)" />
      </symbol>
      <symbol id="icon-user" viewBox="0 0 24 24">
        <use href="#User" transform="translate(-19, -20)" />
      </symbol>

      <!-- Common Icons -->
      <symbol id="icon-tim-kiem" viewBox="0 0 24 24">
        <use href="#TimKiem" transform="translate(-244, -20)" />
      </symbol>
      <symbol id="icon-dropdown" viewBox="0 0 24 24">
        <use href="#Down" transform="translate(-64, -328)" />
      </symbol>
      <symbol id="icon-check" viewBox="0 0 24 24">
        <use href="#Check" transform="translate(-152, -372)" />
      </symbol>
      <symbol id="icon-filter-new" viewBox="0 0 24 24">
        <use href="#Group_46597" transform="translate(-284, -108)" />
      </symbol>
      <symbol id="icon-plus" viewBox="0 0 24 24">
        <use href="#Group_47362" transform="translate(-460, -416)" />
      </symbol>
      <symbol id="icon-plus-2" viewBox="0 0 24 24">
        <use href="#Group_47372" transform="translate(-196, -416)" />
      </symbol>

      <symbol id="icon-excel" viewBox="0 0 24 24">
        <use href="#Group_46597" transform="translate(-284, -108)" />
      </symbol>
      <symbol id="icon-delete" viewBox="0 0 24 24">
        <use href="#Group_47347" transform="translate(-460, -108)" />
      </symbol>
      <symbol id="icon-delete-black" viewBox="0 0 24 24">
        <use href="#Group_46553" transform="translate(-1226, 146)" />
      </symbol>
      <symbol id="icon-edit" viewBox="0 0 24 24">
        <use href="#edit" transform="translate(-152, -65)" />
      </symbol>
      <symbol id="icon-copy" viewBox="0 0 24 24">
        <use href="#MoTrangMoi" transform="translate(260, 255)" />
      </symbol>
      <symbol id="icon-close" viewBox="0 0 24 24">
        <use href="#Xoa_Delete" transform="translate(-284, -240)" />
      </symbol>
      <symbol id="icon-logo" viewBox="0 0 36 36">
        <use href="#Group_47447" transform="translate(-20, -680)" />
      </symbol>
      
      <symbol id="icon-checkbox-check" viewBox="0 0 24 24">
        <use href="#Check" transform="translate(-152, -372)" />
      </symbol>

      <!-- Additional Icons -->
      <symbol id="icon-ok" viewBox="0 0 24 24">
        <use href="#OK" transform="translate(-64, -504)" />
      </symbol>
      <symbol id="icon-off" viewBox="0 0 24 24">
        <use href="#Off" transform="translate(-328, -372)" />
      </symbol>
      <symbol id="icon-on" viewBox="0 0 24 24">
        <use href="#On" transform="translate(-284, -372)" />
      </symbol>
      <symbol id="icon-uncheck-disable" viewBox="0 0 24 24">
        <use href="#Uncheck-Disable" transform="translate(-240, -372)" />
      </symbol>
      <symbol id="icon-both" viewBox="0 0 24 24">
        <use href="#both" transform="translate(-196, -372)" />
      </symbol>
      <symbol id="icon-uncheck" viewBox="0 0 24 24">
        <use href="#Uncheck" transform="translate(-108, -372)" />
      </symbol>
      <symbol id="icon-xoa-chip" viewBox="0 0 24 24">
        <use href="#Xoa-Chip" transform="translate(-328, -240)" />
      </symbol>
      <symbol id="icon-left-s" viewBox="0 0 24 24">
        <use href="#Left-S" transform="translate(-196, -240)" />
      </symbol>
      <symbol id="icon-left-s-2" viewBox="0 0 24 24">
        <use href="#Left-s-2" transform="translate(-196, -284)" />
      </symbol>
      <symbol id="icon-right-s" viewBox="0 0 24 24">
        <use href="#Right-S" transform="translate(-240, -284)" />
      </symbol>
      <symbol id="icon-sap-xep" viewBox="0 0 24 24">
        <use href="#SapXep" transform="translate(-460, -20)" />
      </symbol>
      <symbol id="icon-more-2" viewBox="0 0 24 24">
        <use href="#More-2" transform="translate(-416, -20)" />
      </symbol>
      <symbol id="icon-more-1" viewBox="0 0 24 24">
        <use href="#More-1" transform="translate(-372, -20)" />
      </symbol>
      <symbol id="icon-up" viewBox="0 0 24 24">
        <use href="#Up" transform="translate(-20, -328)" />
      </symbol>
      <symbol id="icon-left" viewBox="0 0 24 24">
        <use href="#Left" transform="translate(-152, -328)" />
      </symbol>
      <symbol id="icon-right" viewBox="0 0 24 24">
        <use href="#Right" transform="translate(-108, -328)" />
      </symbol>
      <symbol id="icon-left-2" viewBox="0 0 24 24">
        <use href="#Left-2" transform="translate(-20, -240)" />
      </symbol>
      <symbol id="icon-left-3" viewBox="0 0 24 24">
        <use href="#Left-3" transform="translate(-20, -284)" />
      </symbol>
      <symbol id="icon-right-2" viewBox="0 0 24 24">
        <use href="#Right-2" transform="translate(-64, -240)" />
      </symbol>
      <symbol id="icon-right-3" viewBox="0 0 24 24">
        <use href="#Right-3" transform="translate(-64, -284)" />
      </symbol>
      <symbol id="icon-top" viewBox="0 0 24 24">
        <use href="#Top" transform="translate(-108, -240)" />
      </symbol>
      <symbol id="icon-top-2" viewBox="0 0 24 24">
        <use href="#Top-2" transform="translate(-108, -284)" />
      </symbol>
      <symbol id="icon-top-3" viewBox="0 0 24 24">
        <use href="#Top-3" transform="translate(-284, -284)" />
      </symbol>
      <symbol id="icon-bottom" viewBox="0 0 24 24">
        <use href="#Bottom" transform="translate(-152, -240)" />
      </symbol>
      <symbol id="icon-bottom-2" viewBox="0 0 24 24">
        <use href="#Bottom-2" transform="translate(-152, -284)" />
      </symbol>
      <symbol id="icon-bottom-3" viewBox="0 0 24 24">
        <use href="#Bottom-3" transform="translate(-328, -284)" />
      </symbol>
      <symbol id="icon-right-s-2" viewBox="0 0 24 24">
        <use href="#Right-S-2" transform="translate(-240, -240)" />
      </symbol>
      <symbol id="icon-back" viewBox="0 0 24 24">
        <use href="#Back" transform="translate(-372, -240)" />
      </symbol>
      <symbol id="icon-cai-dat" viewBox="0 0 24 24">
        <use href="#CaiDat" transform="translate(-112, -23)" />
      </symbol>
      <symbol id="icon-calendar" viewBox="0 0 24 24">
        <use href="#Group_47366" transform="translate(-284, -64)" />
      </symbol>
      <symbol id="icon-combo" viewBox="0 0 24 24">
        <use href="#Combo" transform="translate(-372, -372)" />
      </symbol>
      <symbol id="icon-uncombo" viewBox="0 0 24 24">
        <use href="#Uncombo" transform="translate(-416, -372)" />
      </symbol>
      <symbol id="icon-xoa-icon" viewBox="0 0 24 24">
        <use href="#Xoa-Icon" transform="translate(-240, -416)" />
      </symbol>
      <symbol id="icon-l-icon" viewBox="0 0 24 24">
        <use href="#L-Icon" transform="translate(-284, -416)" />
      </symbol>
      <symbol id="icon-r-icon" viewBox="0 0 24 24">
        <use href="#R-Icon" transform="translate(-328, -416)" />
      </symbol>
      <symbol id="icon-show" viewBox="0 0 24 24">
        <use href="#Show" transform="translate(-20, -372)" />
      </symbol>
      <symbol id="icon-hide" viewBox="0 0 24 24">
        <use href="#Hide" transform="translate(-64, -372)" />
      </symbol>
      <symbol id="icon-cai-dat-fill" viewBox="0 0 24 24">
        <use href="#CaiDat-Fill" transform="translate(-156, -23)" />
      </symbol>
      <symbol id="icon-an-hien-cot" viewBox="0 0 24 24">
        <use href="#An_HienCot" transform="translate(1399.5, -51)" />
      </symbol>
      <symbol id="icon-lock" viewBox="0 0 24 24">
        <use href="#Group_8" transform="translate(-328, -106)" />
      </symbol>
      <symbol id="icon-ho-tro-2" viewBox="0 0 24 24">
        <use href="#HoTro-2" transform="translate(-68, -68)" />
      </symbol>
      <symbol id="icon-download" viewBox="0 0 24 24">
        <use href="#Download" transform="translate(-200, -66)" />
      </symbol>
      <symbol id="icon-copy-file" viewBox="0 0 24 24">
        <use href="#Group_47343" transform="translate(-327.5, -62.5)" />
      </symbol>
      <symbol id="icon-warning" viewBox="0 0 24 24">
        <use href="#Waring-1" transform="translate(-415, -64)" />
      </symbol>
      <symbol id="icon-sap-xep-2" viewBox="0 0 24 24">
        <use href="#SapXep-2" transform="translate(-196, -108)" />
      </symbol>
      <symbol id="icon-in-ma-vach" viewBox="0 0 24 24">
        <use href="#In-MaVach" transform="translate(-20, -460)" />
      </symbol>
      <symbol id="icon-in-the" viewBox="0 0 24 24">
        <use href="#In-The" transform="translate(-20, -504)" />
      </symbol>
      <symbol id="icon-dieu-chuyen" viewBox="0 0 24 24">
        <use href="#DieuChuyen" transform="translate(-64, -460)" />
      </symbol>
      <symbol id="icon-refresh" viewBox="0 0 24 24">
        <use href="#Nap_Refresh" transform="translate(-108, -64)" />
      </symbol>
      <symbol id="icon-horrizon" viewBox="0 0 24 24">
        <use href="#Horrizon" transform="translate(-108, -416)" />
      </symbol>
      <symbol id="icon-vertical" viewBox="0 0 24 24">
        <use href="#Vertical" transform="translate(-64, -416)" />
      </symbol>
      <symbol id="icon-list-view" viewBox="0 0 24 24">
        <use href="#List-View" transform="translate(-20, -416)" />
      </symbol>
      <symbol id="icon-bar" viewBox="0 0 24 24">
        <use href="#Bar" transform="translate(-416, -196)" />
      </symbol>
      <symbol id="icon-bar-2" viewBox="0 0 24 24">
        <use href="#Bar-2" transform="translate(-416, -152)" />
      </symbol>
      <symbol id="icon-menu-to" viewBox="0 0 24 24">
        <use href="#Menu-To" transform="translate(-460, -152)" />
      </symbol>
      <symbol id="icon-menu-nho" viewBox="0 0 24 24">
        <use href="#Menu-Nho" transform="translate(-460, -196)" />
      </symbol>
      <symbol id="icon-ok-2" viewBox="0 0 24 24">
        <use href="#OK-2" transform="translate(-152, -416)" />
      </symbol>
      <symbol id="icon-filter-old" viewBox="0 0 24 24">
        <use href="#Group_47342" transform="translate(-240, -64)" />
      </symbol>
      <symbol id="icon-cross" viewBox="0 0 24 24">
        <use href="#Group_47367" transform="translate(-372, -284)" />
      </symbol>
      <symbol id="icon-delete-table" viewBox="0 0 24 24">
        <use href="#Group_47371" transform="translate(-240, -108)" />
      </symbol>
    </defs>
  </svg>
</template>

<script setup>
import qltsIconSvg from '@/assets/icon/qlts-icon.svg?raw'
</script>
