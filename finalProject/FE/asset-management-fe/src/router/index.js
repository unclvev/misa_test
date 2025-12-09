import { createRouter, createWebHistory } from 'vue-router'
import AssetListView from '../views/AssetListView.vue'
import IconView from '../views/IconView.vue'
import DialogDemoView from '../views/DialogDemoView.vue'
import IncreaseView from '../views/IncreaseView.vue'
import EditIncreaseVoucherView from '../views/EditIncreaseVoucherView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'assets',
      component: AssetListView,
    },
    {
      path: '/assets',
      name: 'asset-list',
      component: AssetListView,
    },
    {
      path: '/increase',
      name: 'increase',
      component: IncreaseView,
    },
    {
      path: '/increase/edit/:id',
      name: 'edit-increase-voucher',
      component: EditIncreaseVoucherView,
      props: true,
    },
    {
      path: '/icons',
      name: 'icons',
      component: IconView,
    },
    {
      path: '/dialogs',
      name: 'dialogs',
      component: DialogDemoView,
    },
  ],
})

export default router
