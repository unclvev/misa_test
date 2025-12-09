<template>
  <MDialog
    :model-value="modelValue"
    :message="message"
    @update:model-value="handleUpdate"
    @confirm="handleDelete"
    @cancel="handleCancel"
  >
    <template #footer>
      <MButton variant="outline" @click="handleCancel">Không</MButton>
      <MButton variant="main" @click="handleDelete">Xóa</MButton>
    </template>
  </MDialog>
</template>

<script setup>
import { computed } from 'vue'
import MDialog from './MDialog.vue'
import MButton from '../ui/MButton.vue'

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  assetCode: {
    type: String,
    default: ''
  },
  assetName: {
    type: String,
    default: ''
  }
})

const emit = defineEmits(['update:modelValue', 'confirm', 'cancel'])

const message = computed(() => {
  if (props.assetCode && props.assetName) {
    return `Bạn có muốn xóa tài sản ${props.assetCode} - ${props.assetName}?`
  }
  return 'Bạn có muốn xóa tài sản này?'
})

const handleUpdate = (value) => {
  emit('update:modelValue', value)
}

const handleDelete = () => {
  emit('confirm')
}

const handleCancel = () => {
  emit('cancel')
}
</script>

