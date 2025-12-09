<template>
  <div class="m-search">
    <svg
      width="16"
      height="16"
      viewBox="0 0 24 24"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      class="m-search__icon"
    >
      <use href="#icon-tim-kiem" />
    </svg>
    <input
      :class="[
        'm-search__input',
        {
          'm-search__input--disabled': disabled
        }
      ]"
      type="text"
      :placeholder="placeholder"
      :value="modelValue"
      :disabled="disabled"
      @input="handleInput"
      @focus="handleFocus"
      @blur="handleBlur"
    />
  </div>
</template>

<script setup>
const props = defineProps({
  modelValue: {
    type: String,
    default: ''
  },
  placeholder: {
    type: String,
    default: 'Tìm kiếm tài sản'
  },
  disabled: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['update:modelValue', 'focus', 'blur'])

const handleInput = (event) => {
  emit('update:modelValue', event.target.value)
}

const handleFocus = (event) => {
  emit('focus', event)
}

const handleBlur = (event) => {
  emit('blur', event)
}
</script>

<style scoped>
.m-search {
  position: relative;
  width: 100%;
  display: flex;
  align-items: center;
}

.m-search__icon {
  position: absolute;
  left: 12px;
  color: #9e9e9e;
  pointer-events: none;
  z-index: 1;
}

.m-search__input {
  width: 100%;
  height: 35px;
  padding: 8px 12px 8px 36px;
  border: 1px solid #afafaf;
  border-radius: 3px;
  font-size: 13px;
  font-family: 'Roboto', sans-serif;
  font-style: italic;
  transition: all 0.2s;
  background-color: #ffffff;
  color: rgba(0, 16, 49, 0.54);
  box-sizing: border-box;
}

.m-search__input::placeholder {
  color: rgba(0, 16, 49, 0.54);
}

.m-search__input:hover:not(:disabled) {
  border-color: #90caf9;
}

.m-search__input:active:not(:disabled) {
  border-color: #1aa4c8;
  color: #1aa4c8;
}

.m-search__input:focus {
  outline: none;
  border-color: #1aa4c8;
  color: #001031;
  font-style: normal;
  box-shadow: 0 0 0 2px rgba(26, 164, 200, 0.2);
}

.m-search__input--disabled {
  background-color: #f5f5f5;
  border-color: #e0e0e0;
  border-style: dashed;
  cursor: not-allowed;
  opacity: 0.6;
}
</style>

