<template>
  <label
    :class="[
      'm-toggle',
      {
        'm-toggle--checked': modelValue,
        'm-toggle--disabled': disabled
      }
    ]"
  >
    <input
      type="checkbox"
      :checked="modelValue"
      :disabled="disabled"
      @change="handleChange"
      class="m-toggle__input"
    />
    <span class="m-toggle__slider"></span>
  </label>
</template>

<script setup>
const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  disabled: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['update:modelValue', 'change'])

const handleChange = (event) => {
  emit('update:modelValue', event.target.checked)
  emit('change', event.target.checked)
}
</script>

<style scoped>
.m-toggle {
  position: relative;
  display: inline-block;
  width: 44px;
  height: 24px;
  cursor: pointer;
}

.m-toggle--disabled {
  cursor: not-allowed;
  opacity: 0.6;
}

.m-toggle__input {
  position: absolute;
  opacity: 0;
  width: 0;
  height: 0;
}

.m-toggle__slider {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: #9e9e9e;
  border-radius: 24px;
  transition: all 0.3s;
}

.m-toggle__slider::before {
  content: '';
  position: absolute;
  height: 18px;
  width: 18px;
  left: 3px;
  bottom: 3px;
  background-color: #ffffff;
  border-radius: 50%;
  transition: all 0.3s;
}

.m-toggle--checked .m-toggle__slider {
  background-color: #1aa4c8;
}

.m-toggle--checked .m-toggle__slider::before {
  transform: translateX(20px);
}

.m-toggle--disabled .m-toggle__slider {
  background-color: #e0e0e0;
}

.m-toggle--disabled.m-toggle--checked .m-toggle__slider {
  background-color: #e0e0e0;
}
</style>

