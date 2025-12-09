<template>
  <MDialog
    :model-value="modelValue"
    :message="message"
    @update:model-value="handleUpdate"
    @confirm="handleSave"
    @cancel="handleCancel"
  >
    <template #footer>
      <MButton variant="outline" @click="handleCancel">Hủy bỏ</MButton>
      <MButton variant="sub" @click="handleDontSave">Không lưu</MButton>
      <MButton variant="main" @click="handleSave">Lưu</MButton>
    </template>
  </MDialog>
</template>

<script setup>
import MDialog from './MDialog.vue'
import MButton from '../ui/MButton.vue'

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  message: {
    type: String,
    default: 'Thông tin thay đổi sẽ không được cập nhật nếu bạn không lưu. Bạn có muốn lưu các thay đổi này?'
  }
})

const emit = defineEmits(['update:modelValue', 'save', 'dont-save', 'cancel'])

const handleUpdate = (value) => {
  emit('update:modelValue', value)
}

const handleSave = () => {
  emit('save')
}

const handleDontSave = () => {
  emit('dont-save')
  emit('update:modelValue', false)
}

const handleCancel = () => {
  emit('cancel')
  emit('update:modelValue', false)
}
</script>

