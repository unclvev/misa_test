<template>
  <label
    :class="[
      'm-checkbox',
      {
        'm-checkbox--checked': modelValue,
        'm-checkbox--disabled': disabled
      }
    ]"
  >
    <input
      type="checkbox"
      :checked="modelValue"
      :disabled="disabled"
      @change="handleChange"
      class="m-checkbox__input"
    />
    <span class="m-checkbox__box">
      <svg
        v-if="modelValue"
        width="12"
        height="12"
        viewBox="0 0 24 24"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
        class="m-checkbox__check"
      >
        <use href="#icon-check" />
      </svg>
    </span>
    <span v-if="$slots.default" class="m-checkbox__label">
      <slot></slot>
    </span>
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
.m-checkbox {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  user-select: none;
  position: relative;
  z-index: 10;
}

.m-checkbox--disabled {
  cursor: not-allowed;
  opacity: 0.6;
}

.m-checkbox__input {
  position: absolute;
  opacity: 0;
  width: 0;
  height: 0;
}

.m-checkbox__box {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 20px;
  height: 20px;
  border: 2px solid #9e9e9e;
  border-radius: 4px;
  background-color: #ffffff;
  transition: all 0.2s;
  flex-shrink: 0;
  position: relative;
  z-index: 11;
}

.m-checkbox:hover:not(.m-checkbox--disabled) .m-checkbox__box {
  border-color: #1aa4c8;
  background-color: #e3f2fd;
  box-shadow: 0 0 0 2px rgba(26, 164, 200, 0.2);
}

.m-checkbox--checked .m-checkbox__box {
  background-color: #1aa4c8;
  border-color: #1aa4c8;
}

.m-checkbox--checked:hover:not(.m-checkbox--disabled) .m-checkbox__box {
  box-shadow: 0 0 0 2px rgba(26, 164, 200, 0.2);
}

.m-checkbox--disabled .m-checkbox__box {
  border-color: #e0e0e0;
  background-color: #f5f5f5;
}

.m-checkbox--checked.m-checkbox--disabled .m-checkbox__box {
  background-color: #e0e0e0;
  border-color: #e0e0e0;
}

.m-checkbox__check {
  color: #ffffff;
  stroke-width: 2;
}

.m-checkbox--disabled .m-checkbox__check {
  color: #9e9e9e;
}

.m-checkbox__label {
  font-size: 13px;
  font-family: 'Roboto', sans-serif;
  color: #001031;
}
</style>

